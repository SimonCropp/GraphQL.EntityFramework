namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var field = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(hasId, true, false),
        };
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        if (resolve is not null)
        {
            field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(async context =>
            {
                var fieldContext = BuildContext(context);
                var result = resolve(fieldContext);

                if (result is IQueryable)
                {
                    throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryField.");
                }

                result = result.ApplyGraphQlArguments(hasId, context, omitQueryArguments);
                if (fieldContext.Filters == null)
                {
                    return result;
                }

                return await fieldContext.Filters.ApplyFilter(result, context.UserContext, fieldContext.DbContext, context.User);
            });
        }

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

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
        var field = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(hasId, true, false),
        };

        var includeNames = FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(async context =>
        {
            var fieldContext = BuildContext(context);
            var projected = compiledProjection(context.Source);

            var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
            {
                Projection = projected,
                DbContext = fieldContext.DbContext,
                User = context.User,
                Filters = fieldContext.Filters,
                FieldContext = context
            };

            var result = resolve(projectionContext);

            if (result is IQueryable)
            {
                throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryField.");
            }

            result = result.ApplyGraphQlArguments(hasId, context, omitQueryArguments);
            if (fieldContext.Filters == null)
            {
                return result;
            }

            return await fieldContext.Filters.ApplyFilter(result, context.UserContext, fieldContext.DbContext, context.User);
        });

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }
}