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
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        var compiledProjection = projection.Compile();

        // Get filter-required navigation paths at setup time for reloading if needed
        var filterRequiredNavPaths = GetFilterRequiredNavPathsForReload<TReturn>();

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

                // If filter requires navigation properties, reload the entity with those includes
                if (result != null && filterRequiredNavPaths.Count > 0)
                {
                    result = await ReloadWithFilterNavigations(
                        fieldContext.DbContext,
                        result,
                        filterRequiredNavPaths);
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

        // Store projection expression - flows through to Select expression builder
        IncludeAppender.SetProjectionMetadata(field, projection, typeof(TSource));
        // Also set include metadata as fallback for abstract types where projection can't be built
        var includeNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    /// <summary>
    /// Gets the navigation paths required by filters for reloading entities.
    /// Returns just the navigation parts (not prefixed with field name).
    /// </summary>
    IReadOnlyList<string> GetFilterRequiredNavPathsForReload<TReturn>()
        where TReturn : class
    {
        var filters = resolveFilters?.Invoke(null!);
        if (filters == null)
        {
            return [];
        }

        var requiredProps = filters.GetRequiredFilterProperties<TReturn>();
        var navigationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in requiredProps)
        {
            var lastDot = prop.LastIndexOf('.');
            if (lastDot > 0)
            {
                // e.g., "TravelRequest.GroupOwnerId" -> "TravelRequest"
                var navPath = prop[..lastDot];
                navigationPaths.Add(navPath);
            }
        }

        return [.. navigationPaths];
    }

    /// <summary>
    /// Reloads an entity from the database with the specified navigation properties included.
    /// </summary>
    static async Task<TReturn?> ReloadWithFilterNavigations<TReturn>(
        TDbContext dbContext,
        TReturn entity,
        IReadOnlyList<string> navigationPaths)
        where TReturn : class
    {
        if (navigationPaths.Count == 0)
        {
            return entity;
        }

        // Get the entity's primary key
        var entityType = dbContext.Model.FindEntityType(typeof(TReturn));
        if (entityType == null)
        {
            return entity;
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
        {
            return entity;
        }

        // Get the key values from the entity
        var keyValues = primaryKey.Properties
            .Select(p => p.PropertyInfo?.GetValue(entity) ?? p.FieldInfo?.GetValue(entity))
            .ToArray();

        if (keyValues.Any(v => v == null))
        {
            return entity;
        }

        // Build a query with includes
        IQueryable<TReturn> query = dbContext.Set<TReturn>();

        foreach (var navPath in navigationPaths)
        {
            query = query.Include(navPath);
        }

        // Filter by primary key
        var keyProperties = primaryKey.Properties.ToList();
        if (keyProperties.Count == 1)
        {
            // Single key - use simple Find-like behavior with includes
            var keyProperty = keyProperties[0];
            var parameter = Expression.Parameter(typeof(TReturn), "e");
            var propertyAccess = Expression.Property(parameter, keyProperty.PropertyInfo!);
            var constant = Expression.Constant(keyValues[0]);
            var equals = Expression.Equal(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<TReturn, bool>>(equals, parameter);

            return await query.FirstOrDefaultAsync(lambda);
        }

        // Composite key - need to build combined predicate
        var param = Expression.Parameter(typeof(TReturn), "e");
        Expression? predicate = null;

        for (var i = 0; i < keyProperties.Count; i++)
        {
            var keyProperty = keyProperties[i];
            var propertyAccess = Expression.Property(param, keyProperty.PropertyInfo!);
            var constant = Expression.Constant(keyValues[i]);
            var equals = Expression.Equal(propertyAccess, constant);

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
        }

        var lambdaExpr = Expression.Lambda<Func<TReturn, bool>>(predicate!, param);
        return await query.FirstOrDefaultAsync(lambdaExpr);
    }
}