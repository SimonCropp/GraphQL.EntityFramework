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

    /// <summary>
    /// Batch reloads multiple entities from the database with the specified navigation properties included.
    /// Uses a single query with WHERE Id IN (...) instead of N+1 queries.
    /// </summary>
    static async Task<IReadOnlyList<TReturn>> BatchReloadWithFilterNavigations<TReturn>(
        TDbContext dbContext,
        IEnumerable<TReturn> entities,
        IReadOnlyList<string> navigationPaths)
        where TReturn : class
    {
        var entityList = entities.ToList();
        if (entityList.Count == 0 || navigationPaths.Count == 0)
        {
            return entityList;
        }

        // Get the entity's primary key metadata
        var entityType = dbContext.Model.FindEntityType(typeof(TReturn));
        if (entityType == null)
        {
            return entityList;
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
        {
            return entityList;
        }

        var keyProperties = primaryKey.Properties.ToList();
        if (keyProperties.Count != 1)
        {
            // For composite keys, fall back to individual reloads
            var results = new List<TReturn>();
            foreach (var entity in entityList)
            {
                var reloaded = await ReloadWithFilterNavigations(dbContext, entity, navigationPaths);
                if (reloaded != null)
                {
                    results.Add(reloaded);
                }
            }
            return results;
        }

        // Single key - can use IN clause
        var keyProperty = keyProperties[0];
        var keyValues = entityList
            .Select(e => keyProperty.PropertyInfo?.GetValue(e) ?? keyProperty.FieldInfo?.GetValue(e))
            .Where(v => v != null)
            .ToList();

        if (keyValues.Count == 0)
        {
            return entityList;
        }

        // Build a query with includes
        IQueryable<TReturn> query = dbContext.Set<TReturn>();

        foreach (var navPath in navigationPaths)
        {
            query = query.Include(navPath);
        }

        // Build WHERE Id IN (...) predicate
        var parameter = Expression.Parameter(typeof(TReturn), "e");
        var propertyAccess = Expression.Property(parameter, keyProperty.PropertyInfo!);

        // Create a list of the key type and use Contains
        var keyType = keyProperty.ClrType;
        var typedKeyValues = typeof(Enumerable)
            .GetMethod("Cast")!
            .MakeGenericMethod(keyType)
            .Invoke(null, [keyValues])!;
        var keyList = typeof(Enumerable)
            .GetMethod("ToList")!
            .MakeGenericMethod(keyType)
            .Invoke(null, [typedKeyValues])!;

        var containsMethod = typeof(List<>)
            .MakeGenericType(keyType)
            .GetMethod("Contains", [keyType])!;

        var containsCall = Expression.Call(
            Expression.Constant(keyList),
            containsMethod,
            propertyAccess);

        var lambda = Expression.Lambda<Func<TReturn, bool>>(containsCall, parameter);

        return await query.Where(lambda).ToListAsync();
    }
}