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
        builder.ResolveAsync(async context =>
        {
            var efFieldContext = BuildContext(context);
            var projected = compiledProjection(context.Source);

            var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
            {
                Projection = projected,
                DbContext = efFieldContext.DbContext,
                User = context.User,
                Filters = efFieldContext.Filters,
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

            enumerable = enumerable.ApplyGraphQlArguments(hasId, context, omitQueryArguments);
            if (efFieldContext.Filters != null)
            {
                enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext, efFieldContext.DbContext, context.User);
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

        field.AddWhereArgument(hasId);
        return builder;
    }

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? itemGraphType = null)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var builder = ConnectionBuilderEx<TSource>.Build(name, itemGraphType);

        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection);

        var hasId = keyNames.ContainsKey(typeof(TReturn));

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = ConnectionBuilderEx<TSource>.NonNullConnectionType(itemGraphType);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(hasId);
        return builder;
    }
}
