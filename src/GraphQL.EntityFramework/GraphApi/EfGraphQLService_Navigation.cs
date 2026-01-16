namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    [Obsolete("Use the projection-based overload instead")]
    public FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var field = new FieldType
        {
            Name = name,
            Type = graphType
        };
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        if (resolve is not null)
        {
            field.Resolver = new FuncFieldResolver<TSource, TReturn?>(
                async context =>
                {
                    var fieldContext = BuildContext(context);

                    TReturn? result;
                    try
                    {
                        result = resolve(fieldContext);
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

                    if (fieldContext.Filters == null ||
                        await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, result))
                    {
                        return result;
                    }

                    return null;
                });
        }

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

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

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for cases where projection processing doesn't find navigations
        var includeNames = FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

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

                if (fieldContext.Filters == null ||
                    await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, result))
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

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback
        var includeNames = FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }
}