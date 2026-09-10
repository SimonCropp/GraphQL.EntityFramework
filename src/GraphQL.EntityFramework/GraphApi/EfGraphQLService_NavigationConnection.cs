namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var builder = ConnectionBuilderEx<TSource>.Build(name, itemGraphType);

        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection);

        var compiledProjection = projection.Compile();

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var names = GetKeyNames<TReturn>();
        builder.ResolveAsync(async context =>
        {
            // Runs once per parent row. Building a ResolveEfFieldContext here copied every property
            // of the GraphQL.NET context, which forced the lazily computed ones, SubFields, Path,
            // ResponsePath, Parent and Arguments, to be computed and allocated per row, when all
            // this needs is the DbContext and the filters.
            var dbContext = ResolveDbContext(context);
            var filters = ResolveFilters(context);
            var projected = compiledProjection(context.Source);

            var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
            {
                Projection = projected,
                DbContext = dbContext,
                User = context.User,
                Filters = filters,
                FieldContext = context
            };

            IEnumerable<TReturn> enumerable;
            try
            {
                enumerable = resolve(projectionContext);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                throw new(
                    $"""
                     Failed to execute query for field `{name}`
                     TSource: {typeof(TSource).FullName}
                     TReturn: {typeof(TReturn).FullName}
                     """,
                    exception);
            }

            if (enumerable is IQueryable)
            {
                throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryConnectionField.");
            }

            // A field selected without arguments has nothing to apply, so the argument reads
            // and the push down lookup are skipped
            if (ArgumentReader.HasArguments(context))
            {
                var applied = ReferenceEquals(enumerable, projected) && PushDown.IsApplied(context);
                enumerable = enumerable.ApplyGraphQlArguments(names, context, omitQueryArguments, applied);
            }

            if (filters != null)
            {
                enumerable = await filters.ApplyFilter(enumerable, context.UserContext, dbContext, context.User);
            }

            var page = enumerable.ToList();

            return ConnectionConverter.ApplyConnectionContext(
                page,
                context.First,
                context.After,
                context.Last,
                context.Before);
        });

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = ConnectionBuilderEx<TSource>.NonNullConnectionType(itemGraphType);
        var field = graph.AddField(builder.FieldType);

        // The arguments were added regardless, so omitQueryArguments only stopped them being
        // applied while the schema still advertised them
        if (!omitQueryArguments)
        {
            field.AddWhereArgument(typeof(TReturn), hasId);
        }

        return builder;
    }

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? itemGraphType = null)
        where TReturn : class
    {
        // On an object type a field with no resolver falls back to GraphQL.NET's name resolver,
        // which returns the raw property rather than a connection. The projection is what should
        // be paged, so it gets the resolver AutoMap uses. An interface field declares the shape only.
        if (graph is IObjectGraphType)
        {
            return AddNavigationConnectionField(graph, name, projection, _ => _.Projection ?? [], itemGraphType);
        }

        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var builder = ConnectionBuilderEx<TSource>.Build(name, itemGraphType);

        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection);

        var hasId = keyNames.ContainsKey(typeof(TReturn));

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = ConnectionBuilderEx<TSource>.NonNullConnectionType(itemGraphType);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(typeof(TReturn), hasId);
        return builder;
    }
}
