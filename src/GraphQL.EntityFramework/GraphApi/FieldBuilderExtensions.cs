namespace GraphQL.EntityFramework;

/// <summary>
/// Extension methods for FieldBuilder to support projection-based resolvers.
/// </summary>
public static class FieldBuilderExtensions
{
    /// <summary>
    /// Resolves field value using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TReturn">The return type</typeparam>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="builder">The field builder</param>
    /// <param name="graphQlService">The service used to resolve the DbContext and filters</param>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Function to resolve field value from projected data</param>
    /// <returns>The field builder for chaining</returns>
    /// <remarks>
    /// Use this method instead of Resolve() when you need to access navigation
    /// properties. The projection ensures the required data is loaded from the
    /// database.
    /// </remarks>
    public static FieldBuilder<TSource, TReturn> Resolve<TDbContext, TSource, TReturn, TProjection>(
        this FieldBuilder<TSource, TReturn> builder,
        IEfGraphQLService<TDbContext> graphQlService,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn> resolve)
        where TDbContext : DbContext
    {
        ValidateProjection(projection);

        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        // A sync resolve completes synchronously unless a filter has to run, so the resolver
        // returns a ValueTask rather than allocating an async state machine per field per row
        field.Resolver = new FuncFieldResolver<TSource, TReturn>(
            context =>
            {
                var projectionContext = BuildProjectionContext(graphQlService, compiledProjection, context);

                TReturn result;
                try
                {
                    result = resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute projection-based resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                return ApplyFilters(projectionContext.Filters, context, projectionContext.DbContext, result);
            });

        return builder;
    }

    /// <summary>
    /// Resolves field value asynchronously using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TReturn">The return type</typeparam>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="builder">The field builder</param>
    /// <param name="graphQlService">The service used to resolve the DbContext and filters</param>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Async function to resolve field value from projected data</param>
    /// <returns>The field builder for chaining</returns>
    /// <remarks>
    /// Use this method instead of ResolveAsync() when you need to access navigation
    /// properties. The projection ensures the required data is loaded from the
    /// database.
    /// </remarks>
    public static FieldBuilder<TSource, TReturn> ResolveAsync<TDbContext, TSource, TReturn, TProjection>(
        this FieldBuilder<TSource, TReturn> builder,
        IEfGraphQLService<TDbContext> graphQlService,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, Task<TReturn>> resolve)
        where TDbContext : DbContext
    {
        ValidateProjection(projection);

        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, TReturn>(
            async context =>
            {
                var projectionContext = BuildProjectionContext(graphQlService, compiledProjection, context);

                TReturn result;
                try
                {
                    result = await resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute projection-based async resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                return await ApplyFilters(projectionContext.Filters, context, projectionContext.DbContext, result);
            });

        return builder;
    }

    /// <summary>
    /// Resolves a list of field values using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TReturn">The return item type</typeparam>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="builder">The field builder</param>
    /// <param name="graphQlService">The service used to resolve the DbContext and filters</param>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Function to resolve list of field values from projected data</param>
    /// <returns>The field builder for chaining</returns>
    /// <remarks>
    /// Use this method instead of Resolve() when you need to access navigation
    /// properties and return a list. The projection ensures the required data is loaded
    /// from the database.
    /// </remarks>
    public static FieldBuilder<TSource, IEnumerable<TReturn>> ResolveList<TDbContext, TSource, TReturn, TProjection>(
        this FieldBuilder<TSource, IEnumerable<TReturn>> builder,
        IEfGraphQLService<TDbContext> graphQlService,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve)
        where TDbContext : DbContext
    {
        ValidateProjection(projection);

        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            context =>
            {
                var projectionContext = BuildProjectionContext(graphQlService, compiledProjection, context);

                IEnumerable<TReturn> result;
                try
                {
                    result = resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute projection-based list resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                return ApplyListFilters(projectionContext.Filters, context, projectionContext.DbContext, result);
            });

        return builder;
    }

    /// <summary>
    /// Resolves a list of field values asynchronously using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TReturn">The return item type</typeparam>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="builder">The field builder</param>
    /// <param name="graphQlService">The service used to resolve the DbContext and filters</param>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Async function to resolve list of field values from projected data</param>
    /// <returns>The field builder for chaining</returns>
    /// <remarks>
    /// Use this method instead of ResolveAsync() when you need to access navigation
    /// properties and return a list. The projection ensures the required data is loaded
    /// from the database.
    /// </remarks>
    public static FieldBuilder<TSource, IEnumerable<TReturn>> ResolveListAsync<TDbContext, TSource, TReturn, TProjection>(
        this FieldBuilder<TSource, IEnumerable<TReturn>> builder,
        IEfGraphQLService<TDbContext> graphQlService,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, Task<IEnumerable<TReturn>>> resolve)
        where TDbContext : DbContext
    {
        ValidateProjection(projection);

        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                var projectionContext = BuildProjectionContext(graphQlService, compiledProjection, context);

                IEnumerable<TReturn> result;
                try
                {
                    result = await resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute projection-based async list resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                return await ApplyListFilters(projectionContext.Filters, context, projectionContext.DbContext, result);
            });

        return builder;
    }

    static ResolveProjectionContext<TDbContext, TProjection> BuildProjectionContext<TDbContext, TSource, TProjection>(
        IEfGraphQLService<TDbContext> graphQlService,
        Func<TSource, TProjection> compiledProjection,
        IResolveFieldContext<TSource> context)
        where TDbContext : DbContext =>
        new()
        {
            DbContext = graphQlService.ResolveDbContext(context),
            Filters = graphQlService.ResolveFilters(context),
            Projection = compiledProjection(context.Source),
            User = context.User,
            FieldContext = context
        };

    static ValueTask<TReturn?> ApplyFilters<TDbContext, TReturn>(
        Filters<TDbContext>? filters,
        IResolveFieldContext context,
        TDbContext dbContext,
        TReturn result)
        where TDbContext : DbContext
    {
        // Value types don't support filtering - return as-is
        if (typeof(TReturn).IsValueType ||
            filters is not { HasFilters: true } ||
            result is null)
        {
            return new(result);
        }

        return ApplyFiltersAsync(filters, context, dbContext, result);
    }

    /// <summary>
    /// The items of a list resolve are filtered the same way every other list path filters them.
    /// They were returned as is, so a filter that excluded an item elsewhere let it through here.
    /// </summary>
    static ValueTask<IEnumerable<TReturn>?> ApplyListFilters<TDbContext, TReturn>(
        Filters<TDbContext>? filters,
        IResolveFieldContext context,
        TDbContext dbContext,
        IEnumerable<TReturn> result)
        where TDbContext : DbContext
    {
        if (typeof(TReturn).IsValueType ||
            filters is not { HasFilters: true })
        {
            return new(result);
        }

        return ApplyListFiltersAsync(filters, context, dbContext, result);
    }

    static async ValueTask<IEnumerable<TReturn>?> ApplyListFiltersAsync<TDbContext, TReturn>(
        Filters<TDbContext> filters,
        IResolveFieldContext context,
        TDbContext dbContext,
        IEnumerable<TReturn> result)
        where TDbContext : DbContext
    {
        var list = new List<TReturn>();
        foreach (var item in result)
        {
            if (item is null ||
                await filters.ShouldInclude(context.UserContext, dbContext, context.User, (object) item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    static async ValueTask<TReturn?> ApplyFiltersAsync<TDbContext, TReturn>(
        Filters<TDbContext> filters,
        IResolveFieldContext context,
        TDbContext dbContext,
        TReturn result)
        where TDbContext : DbContext
    {
        // For reference types, apply filters if available. Matched on the runtime type of the
        // result, so a field typed as object is filtered the same as a typed one.
        if (!await filters.ShouldInclude(context.UserContext, dbContext, context.User, (object)result!))
        {
            return default;
        }

        return result;
    }

    /// <summary>
    /// Sets projection metadata on a field without wrapping the resolver.
    /// This ensures the parent query loads the required fields from the database,
    /// even when the field has its own custom resolver.
    /// </summary>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TReturn">The return type</typeparam>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="builder">The field builder</param>
    /// <param name="projection">Expression describing the required entity data</param>
    /// <returns>The field builder for chaining</returns>
    public static FieldBuilder<TSource, TReturn> WithProjection<TSource, TReturn, TProjection>(
        this FieldBuilder<TSource, TReturn> builder,
        Expression<Func<TSource, TProjection>> projection)
    {
        ValidateProjection(projection);
        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection);
        return builder;
    }

    static void ValidateProjection<TSource, TProjection>(Expression<Func<TSource, TProjection>> projection)
    {
        // Detect identity projection: _ => _
        if (projection.Body is ParameterExpression parameter &&
            parameter == projection.Parameters[0])
        {
            throw new ArgumentException(
                "Identity projection '_ => _' is not allowed. If only access to primary key or foreign key properties, use the regular Resolve() method instead. If required to access navigation properties, specify them in the projection (e.g., '_ => _.Parent').",
                nameof(projection));
        }

        // Note: Scalar projections are allowed - they're useful for ensuring scalar properties
        // are loaded from the database and can be transformed in the resolver
    }
}
