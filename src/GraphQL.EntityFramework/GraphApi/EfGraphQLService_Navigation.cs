namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn?> resolve,
        Type? graphType = null)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var field = new FieldType
        {
            Name = name,
            Type = graphType
        };

        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, TReturn?>(
            async context =>
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

                TReturn? result;
                try
                {
                    result = resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute navigation resolve for field `{name}`
                        GraphType: {graphType.FullName}
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        """,
                        exception);
                }

                if (fieldContext.Filters == null)
                {
                    return result;
                }

                if (await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, result))
                {
                    return result;
                }

                return null;
            });

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TReturn?>> projection,
        Type? graphType = null)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var field = new FieldType
        {
            Name = name,
            Type = graphType
        };

        IncludeAppender.SetProjectionMetadata(field, projection);

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }
}