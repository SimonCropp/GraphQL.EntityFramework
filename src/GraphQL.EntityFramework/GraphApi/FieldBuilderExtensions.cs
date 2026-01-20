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
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn?> resolve)
        where TDbContext : DbContext
        where TReturn : class
    {
        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, field.Name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, TReturn?>(
            async context =>
            {
                // Resolve service from request services
                var executionContext = context.ExecutionContext;
                var requestServices = executionContext.RequestServices ?? executionContext.ExecutionOptions.RequestServices;
               if (requestServices?.GetService(typeof(IEfGraphQLService<TDbContext>)) is not IEfGraphQLService<TDbContext> graphQlService)
                {
                    throw new InvalidOperationException($"IEfGraphQLService<{typeof(TDbContext).Name}> not found in request services. Ensure it's registered in the container.");
                }

                var dbContext = graphQlService.ResolveDbContext(context);
                var filters = ResolveFilters(graphQlService, context);
                var projected = compiledProjection(context.Source);

                var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
                {
                    Projection = projected,
                    DbContext = dbContext,
                    User = context.User,
                    Filters = filters,
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
                        Failed to execute projection-based resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                if (filters == null ||
                    await filters.ShouldInclude(context.UserContext, dbContext, context.User, result))
                {
                    return result;
                }

                return default;
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
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, Task<TReturn?>> resolve)
        where TDbContext : DbContext
        where TReturn : class
    {
        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, field.Name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, TReturn?>(
            async context =>
            {
                // Resolve service from request services
                var executionContext = context.ExecutionContext;
                var requestServices = executionContext.RequestServices ?? executionContext.ExecutionOptions.RequestServices;
               if (requestServices?.GetService(typeof(IEfGraphQLService<TDbContext>)) is not IEfGraphQLService<TDbContext> graphQlService)
                {
                    throw new InvalidOperationException($"IEfGraphQLService<{typeof(TDbContext).Name}> not found in request services. Ensure it's registered in the container.");
                }

                var dbContext = graphQlService.ResolveDbContext(context);
                var filters = ResolveFilters(graphQlService, context);
                var projected = compiledProjection(context.Source);

                var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
                {
                    Projection = projected,
                    DbContext = dbContext,
                    User = context.User,
                    Filters = filters,
                    FieldContext = context
                };

                TReturn? result;
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

                if (filters == null ||
                    await filters.ShouldInclude(context.UserContext, dbContext, context.User, result))
                {
                    return result;
                }

                return default;
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
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve)
        where TDbContext : DbContext
    {
        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, field.Name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                // Resolve service from request services
                var executionContext = context.ExecutionContext;
                var requestServices = executionContext.RequestServices ?? executionContext.ExecutionOptions.RequestServices;
               if (requestServices?.GetService(typeof(IEfGraphQLService<TDbContext>)) is not IEfGraphQLService<TDbContext> graphQlService)
                {
                    throw new InvalidOperationException($"IEfGraphQLService<{typeof(TDbContext).Name}> not found in request services. Ensure it's registered in the container.");
                }

                var dbContext = graphQlService.ResolveDbContext(context);
                var filters = ResolveFilters(graphQlService, context);
                var projected = compiledProjection(context.Source);

                var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
                {
                    Projection = projected,
                    DbContext = dbContext,
                    User = context.User,
                    Filters = filters,
                    FieldContext = context
                };

                IEnumerable<TReturn> result;
                try
                {
                    result = resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new Exception(
                        $"""
                        Failed to execute projection-based list resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                // Note: For list results, we don't apply filters on the collection itself
                // Filters would be applied to individual items if needed
                return result ?? Enumerable.Empty<TReturn>();
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
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, Task<IEnumerable<TReturn>>> resolve)
        where TDbContext : DbContext
    {
        var field = builder.FieldType;

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, field.Name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                // Resolve service from request services
                var executionContext = context.ExecutionContext;
                var requestServices = executionContext.RequestServices ?? executionContext.ExecutionOptions.RequestServices;
               if (requestServices?.GetService(typeof(IEfGraphQLService<TDbContext>)) is not IEfGraphQLService<TDbContext> graphQlService)
                {
                    throw new InvalidOperationException($"IEfGraphQLService<{typeof(TDbContext).Name}> not found in request services. Ensure it's registered in the container.");
                }

                var dbContext = graphQlService.ResolveDbContext(context);
                var filters = ResolveFilters(graphQlService, context);
                var projected = compiledProjection(context.Source);

                var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
                {
                    Projection = projected,
                    DbContext = dbContext,
                    User = context.User,
                    Filters = filters,
                    FieldContext = context
                };

                IEnumerable<TReturn> result;
                try
                {
                    result = await resolve(projectionContext);
                }
                catch (Exception exception)
                {
                    throw new Exception(
                        $"""
                        Failed to execute projection-based async list resolve for field `{field.Name}`
                        TSource: {typeof(TSource).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        """,
                        exception);
                }

                // Note: For list results, we don't apply filters on the collection itself
                // Filters would be applied to individual items if needed
                return result ?? Enumerable.Empty<TReturn>();
            });

        return builder;
    }

    static Filters<TDbContext>? ResolveFilters<TDbContext>(IEfGraphQLService<TDbContext> service, IResolveFieldContext context)
        where TDbContext : DbContext
    {
        // Use reflection to access the protected/internal ResolveFilter method
        // This is a workaround since we can't access it directly
        var serviceType = service.GetType();
        var method = serviceType.GetMethod("ResolveFilter",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method is null)
        {
            return null;
        }

        var genericMethod = method.MakeGenericMethod(context.Source?.GetType() ?? typeof(object));
        return genericMethod.Invoke(service, [context]) as Filters<TDbContext>;
    }
}
