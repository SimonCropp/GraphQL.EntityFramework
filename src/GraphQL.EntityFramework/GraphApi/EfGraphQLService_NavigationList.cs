namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var names = GetKeyNames<TReturn>();
        var field = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(typeof(TReturn), hasId, true, false, orderByStyle, omitQueryArguments),
        };

        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(async context =>
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

            var result = resolve(projectionContext);

            if (result is IQueryable)
            {
                throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryField.");
            }

            // A field selected without arguments has nothing to apply, and that is the common
            // case for a navigation, so the argument reads and the push down lookup are skipped
            if (ArgumentReader.HasArguments(context))
            {
                // The collection arrives with ids, where and orderBy already applied when the parent
                // was loaded through a projection and the resolver returned that collection as is
                var applied = ReferenceEquals(result, projected) && PushDown.IsApplied(context);
                result = result.ApplyGraphQlArguments(names, context, omitQueryArguments, applied);
            }

            if (filters == null)
            {
                return result;
            }

            return await filters.ApplyFilter(result, context.UserContext, dbContext, context.User);
        });

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        // On an object type a field with no resolver falls back to GraphQL.NET's name resolver,
        // which returns the raw property, ignores the arguments the field advertises, and applies
        // no filters. The projection is what should be returned, so it gets the resolver AutoMap
        // uses. An interface field declares the shape only.
        if (graph is IObjectGraphType)
        {
            return AddNavigationListField(graph, name, projection, _ => _.Projection ?? [], itemGraphType, omitQueryArguments);
        }

        Ensure.NotWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var field = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(typeof(TReturn), hasId, true, false, orderByStyle, omitQueryArguments),
        };

        IncludeAppender.SetProjectionMetadata(field, projection);

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }
}