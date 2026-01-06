namespace GraphQL.EntityFramework;

static class SelectExpressionBuilder
{
    static readonly ConcurrentDictionary<string, object> cache = new();
    static readonly ConcurrentDictionary<string, object> cacheWithFilters = new();

    public static bool TryBuild<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out Expression<Func<TEntity, TEntity>>? expression)
        where TEntity : class
    {
        var cacheKey = BuildCacheKey<TEntity>(projection);
        var result = cache.GetOrAdd(
            cacheKey,
            _ => BuildExpression<TEntity>(projection, keyNames)!);

        expression = result as Expression<Func<TEntity, TEntity>>;
        return expression != null;
    }

    public static bool TryBuildWithFilters<TEntity, TDbContext>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IEnumerable<IFilterEntry<TDbContext>> filters,
        [NotNullWhen(true)] out Expression<Func<TEntity, EntityWithFilterData<TEntity>>>? expression)
        where TEntity : class
        where TDbContext : DbContext
    {
        var filterList = filters.ToList();
        if (filterList.Count == 0)
        {
            expression = null;
            return false;
        }

        var cacheKey = BuildCacheKeyWithFilters<TEntity, TDbContext>(projection, filterList);
        var result = cacheWithFilters.GetOrAdd(
            cacheKey,
            _ => BuildExpressionWithFilters<TEntity, TDbContext>(projection, keyNames, filterList)!);

        expression = result as Expression<Func<TEntity, EntityWithFilterData<TEntity>>>;
        return expression != null;
    }

    static Expression<Func<TEntity, EntityWithFilterData<TEntity>>>? BuildExpressionWithFilters<TEntity, TDbContext>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        List<IFilterEntry<TDbContext>> filters)
        where TEntity : class
        where TDbContext : DbContext
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");

        // Build entity bindings (reuse existing logic)
        if (!TryBuildEntityBindings(parameter, entityType, projection, keyNames, out var entityBindings))
        {
            return null;
        }

        var entityInit = Expression.MemberInit(Expression.New(entityType), entityBindings);

        // Build filter data dictionary expression
        // new Dictionary<Type, object> { { typeof(TEntity), filterProjection(x) }, ... }
        var dictType = typeof(Dictionary<Type, object>);
        var dictAddMethod = dictType.GetMethod("Add", [typeof(Type), typeof(object)])!;

        var dictVariable = Expression.Variable(dictType, "filterData");
        var dictNew = Expression.New(dictType);
        var dictAssign = Expression.Assign(dictVariable, dictNew);

        var statements = new List<Expression> { dictAssign };

        foreach (var filter in filters)
        {
            var filterProjection = filter.GetProjectionExpression();
            // Invoke the filter projection with our parameter
            var filterInvoke = Expression.Invoke(filterProjection, parameter);
            // Box the result to object
            var boxed = Expression.Convert(filterInvoke, typeof(object));
            // dict.Add(typeof(TEntity), projectedValue)
            var typeConstant = Expression.Constant(filter.EntityType, typeof(Type));
            var addCall = Expression.Call(dictVariable, dictAddMethod, typeConstant, boxed);
            statements.Add(addCall);
        }

        // Build EntityWithFilterData<TEntity> initialization
        var wrapperType = typeof(EntityWithFilterData<TEntity>);
        var entityProperty = wrapperType.GetProperty("Entity")!;
        var filterDataProperty = wrapperType.GetProperty("FilterData")!;

        var wrapperInit = Expression.MemberInit(
            Expression.New(wrapperType),
            Expression.Bind(entityProperty, entityInit),
            Expression.Bind(filterDataProperty, dictVariable));

        statements.Add(wrapperInit);

        var block = Expression.Block(
            [dictVariable],
            statements);

        return Expression.Lambda<Func<TEntity, EntityWithFilterData<TEntity>>>(block, parameter);
    }

    static bool TryBuildEntityBindings(
        ParameterExpression parameter,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out List<MemberBinding>? bindings)
    {
        bindings = [];
        var addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Always include key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (TryGetProperty(entityType, keyName, out var prop) &&
                prop.CanWrite &&
                addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 2. Add requested scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (TryGetProperty(entityType, fieldName, out var prop) &&
                addedProperties.Add(prop.Name))
            {
                if (!prop.CanWrite)
                {
                    bindings = null;
                    return false;
                }

                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 3. Add navigation properties with nested projections
        foreach (var (navFieldName, navProjection) in projection.Navigations)
        {
            if (!TryGetProperty(entityType, navFieldName, out var prop) ||
                !addedProperties.Add(prop.Name))
            {
                continue;
            }

            var binding = BuildNavigationBinding(parameter, prop, navProjection, keyNames);
            if (binding == null)
            {
                bindings = null;
                return false;
            }
            bindings.Add(binding);
        }

        return true;
    }

    static string BuildCacheKeyWithFilters<TEntity, TDbContext>(
        FieldProjectionInfo projection,
        IEnumerable<IFilterEntry<TDbContext>> filters)
        where TDbContext : DbContext
    {
        var builder = new StringBuilder();
        builder.Append(typeof(TEntity).FullName);
        builder.Append('|');
        BuildProjectionKey(builder, projection);
        builder.Append("|filters:");
        foreach (var filter in filters.OrderBy(f => f.EntityType.FullName))
        {
            builder.Append(filter.EntityType.FullName);
            builder.Append(',');
        }
        return builder.ToString();
    }

    static Expression<Func<TEntity, TEntity>>? BuildExpression<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");
        var bindings = new List<MemberBinding>();
        var addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Always include key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (TryGetProperty(entityType, keyName, out var prop) &&
                prop.CanWrite &&
                addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 2. Add requested scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (TryGetProperty(entityType, fieldName, out var prop) &&
                addedProperties.Add(prop.Name))
            {
                if (!prop.CanWrite)
                {
                    // Read-only property (expression-bodied or database computed column)
                    // Can't use projection - return null to load full entity
                    return null;
                }

                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 3. Add navigation properties with nested projections
        foreach (var (navFieldName, navProjection) in projection.Navigations)
        {
            if (!TryGetProperty(entityType, navFieldName, out var prop) ||
                !addedProperties.Add(prop.Name))
            {
                continue;
            }

            var binding = BuildNavigationBinding(parameter, prop, navProjection, keyNames);
            if (binding == null)
            {
                // Can't project navigation - return null to load full entity
                return null;
            }
            bindings.Add(binding);
        }

        var memberInit = Expression.MemberInit(Expression.New(entityType), bindings);
        return Expression.Lambda<Func<TEntity, TEntity>>(memberInit, parameter);
    }

    static MemberBinding? BuildNavigationBinding(
        ParameterExpression parameter,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames) =>
        navProjection.IsCollection
            ? BuildCollectionNavigationBinding(parameter, prop, navProjection, keyNames)
            : BuildSingleNavigationBinding(parameter, prop, navProjection, keyNames);

    static MemberAssignment? BuildCollectionNavigationBinding(
        ParameterExpression parameter,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;
        var navParam = Expression.Parameter(navType, "n");

        // Build the inner MemberInit for the navigation type
        if (!TryBuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames, out var innerBindings))
        {
            // Can't project navigation - return null to load full entity
            return null;
        }
        var innerMemberInit = Expression.MemberInit(Expression.New(navType), innerBindings);
        var innerLambda = Expression.Lambda(innerMemberInit, navParam);

        // x.Children
        var navAccess = Expression.Property(parameter, prop);

        // Build: x.Children.OrderBy(n => n.Key) to ensure deterministic ordering
        Expression orderedCollection = navAccess;
        if (keyNames.TryGetValue(navType, out var keys) && keys.Count > 0)
        {
            var orderParam = Expression.Parameter(navType, "o");
            if (TryGetProperty(navType, keys[0], out var keyProp))
            {
                var keyAccess = Expression.Property(orderParam, keyProp);
                var keyLambda = Expression.Lambda(keyAccess, orderParam);

                var orderByMethod = EnumerableMethodCache.MakeGenericMethod(
                    EnumerableMethodCache.OrderByMethod,
                    navType,
                    keyProp.PropertyType);

                orderedCollection = Expression.Call(null, orderByMethod, navAccess, keyLambda);
            }
        }

        // Build: x.Children.OrderBy(...).Select(n => new Child { ... })
        var selectMethod = EnumerableMethodCache.MakeGenericMethod(
            EnumerableMethodCache.SelectMethod,
            navType,
            navType);

        var selectCall = Expression.Call(null, selectMethod, orderedCollection, innerLambda);

        // Build: .ToList()
        var toListMethod = EnumerableMethodCache.MakeGenericMethod(
            EnumerableMethodCache.ToListMethod,
            navType);

        var toListCall = Expression.Call(null, toListMethod, selectCall);

        return Expression.Bind(prop, toListCall);
    }

    static MemberBinding? BuildSingleNavigationBinding(
        ParameterExpression parameter,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;

        // x.Parent
        var navAccess = Expression.Property(parameter, prop);

        // x.Parent == null
        var nullCheck = Expression.Equal(navAccess, Expression.Constant(null, navType));

        // Build the MemberInit for the navigation type using navAccess as source
        if (!TryBuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames, out var innerBindings))
        {
            // Can't project navigation - return null to load full entity
            return null;
        }
        var memberInit = Expression.MemberInit(Expression.New(navType), innerBindings);

        // x.Parent == null ? null : new Parent { ... }
        var conditional = Expression.Condition(
            nullCheck,
            Expression.Constant(null, navType),
            memberInit);

        return Expression.Bind(prop, conditional);
    }

    static bool TryBuildNavigationBindings(
        Expression sourceExpression,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out List<MemberBinding>? bindings)
    {
        bindings = [];
        var addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (TryGetProperty(entityType, keyName, out var prop) &&
                prop.CanWrite &&
                addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(sourceExpression, prop)));
            }
        }

        // Add scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (TryGetProperty(entityType, fieldName, out var prop) &&
                addedProperties.Add(prop.Name))
            {
                if (!prop.CanWrite)
                {
                    // Read-only property (expression-bodied or database computed column)
                    // Can't use projection - return false to load full entity
                    bindings = null;
                    return false;
                }

                bindings.Add(Expression.Bind(prop, Expression.Property(sourceExpression, prop)));
            }
        }

        // Add nested navigations recursively
        foreach (var (navFieldName, nestedNavProjection) in projection.Navigations)
        {
            if (!TryGetProperty(entityType, navFieldName, out var prop) ||
                !addedProperties.Add(prop.Name))
            {
                continue;
            }

            if (!TryBuildNestedNavigationBinding(sourceExpression, prop, nestedNavProjection, keyNames, out var binding))
            {
                // Can't project navigation - return false to load full entity
                bindings = null;
                return false;
            }
            bindings.Add(binding);
        }

        return true;
    }

    static bool TryBuildNestedNavigationBinding(
        Expression sourceExpression,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out MemberAssignment? binding)
    {
        binding = null;
        var navType = navProjection.EntityType;

        if (navProjection.IsCollection)
        {
            // sourceExpression.Children
            var navAccess = Expression.Property(sourceExpression, prop);
            var navParam = Expression.Parameter(navType, "n");

            // Build the inner MemberInit
            if (!TryBuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames, out var innerBindings))
            {
                // Can't project navigation - return false to load full entity
                return false;
            }
            var innerMemberInit = Expression.MemberInit(Expression.New(navType), innerBindings);
            var innerLambda = Expression.Lambda(innerMemberInit, navParam);

            // .OrderBy(o => o.Key) to ensure deterministic ordering
            Expression orderedCollection = navAccess;
            if (keyNames.TryGetValue(navType, out var keys) && keys.Count > 0)
            {
                var orderParam = Expression.Parameter(navType, "o");
                if (TryGetProperty(navType, keys[0], out var keyProp))
                {
                    var keyAccess = Expression.Property(orderParam, keyProp);
                    var keyLambda = Expression.Lambda(keyAccess, orderParam);

                    var orderByMethod = EnumerableMethodCache.MakeGenericMethod(
                        EnumerableMethodCache.OrderByMethod,
                        navType,
                        keyProp.PropertyType);

                    orderedCollection = Expression.Call(null, orderByMethod, navAccess, keyLambda);
                }
            }

            // .Select(n => new Child { ... })
            var selectMethod = EnumerableMethodCache.MakeGenericMethod(
                EnumerableMethodCache.SelectMethod,
                navType,
                navType);

            var selectCall = Expression.Call(null, selectMethod, orderedCollection, innerLambda);

            // .ToList()
            var toListMethod = EnumerableMethodCache.MakeGenericMethod(
                EnumerableMethodCache.ToListMethod,
                navType);

            var toListCall = Expression.Call(null, toListMethod, selectCall);

            binding = Expression.Bind(prop, toListCall);
            return true;
        }
        else
        {
            // sourceExpression.Parent
            var navAccess = Expression.Property(sourceExpression, prop);

            // sourceExpression.Parent == null
            var nullCheck = Expression.Equal(navAccess, Expression.Constant(null, navType));

            // Build the MemberInit
            if (!TryBuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames, out var innerBindings))
            {
                // Can't project navigation - return false to load full entity
                return false;
            }
            var memberInit = Expression.MemberInit(Expression.New(navType), innerBindings);

            // sourceExpression.Parent == null ? null : new Parent { ... }
            var conditional = Expression.Condition(
                nullCheck,
                Expression.Constant(null, navType),
                memberInit);

            binding = Expression.Bind(prop, conditional);
            return true;
        }
    }

    static bool TryGetProperty(Type type, string name, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property != null;
    }

    static string BuildCacheKey<TEntity>(FieldProjectionInfo projection)
    {
        var builder = new StringBuilder();
        builder.Append(typeof(TEntity).FullName);
        builder.Append('|');
        BuildProjectionKey(builder, projection);
        return builder.ToString();
    }

    static void BuildProjectionKey(StringBuilder builder, FieldProjectionInfo projection)
    {
        // Sort scalar fields for consistent cache key
        var sortedScalars = projection.ScalarFields.OrderBy(_ => _, StringComparer.OrdinalIgnoreCase);
        builder.Append(string.Join(',', sortedScalars));

        if (projection.Navigations.Count <= 0)
        {
            return;
        }

        builder.Append('{');
        var sortedNavs = projection.Navigations.OrderBy(_ => _.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var (navName, navProjection) in sortedNavs)
        {
            builder.Append(navName);
            builder.Append(':');
            BuildProjectionKey(builder, navProjection.Projection);
            builder.Append(';');
        }
        builder.Append('}');
    }
}
