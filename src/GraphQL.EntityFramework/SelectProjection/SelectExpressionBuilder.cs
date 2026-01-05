namespace GraphQL.EntityFramework;

static class SelectExpressionBuilder
{
    static readonly ConcurrentDictionary<string, object> cache = new();

    public static Expression<Func<TEntity, TEntity>> Build<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
        where TEntity : class
    {
        var cacheKey = BuildCacheKey<TEntity>(projection);
        return (Expression<Func<TEntity, TEntity>>)cache.GetOrAdd(
            cacheKey,
            _ => BuildExpression<TEntity>(projection, keyNames));
    }

    static Expression<Func<TEntity, TEntity>> BuildExpression<TEntity>(
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
                prop.CanWrite &&
                addedProperties.Add(prop.Name))
            {
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
            bindings.Add(binding);
        }

        var memberInit = Expression.MemberInit(Expression.New(entityType), bindings);
        return Expression.Lambda<Func<TEntity, TEntity>>(memberInit, parameter);
    }

    static MemberBinding BuildNavigationBinding(
        ParameterExpression parameter,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames) =>
        navProjection.IsCollection
            ? BuildCollectionNavigationBinding(parameter, prop, navProjection, keyNames)
            : BuildSingleNavigationBinding(parameter, prop, navProjection, keyNames);

    static MemberAssignment BuildCollectionNavigationBinding(
        ParameterExpression parameter,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;
        var navParam = Expression.Parameter(navType, "n");

        // Build the inner MemberInit for the navigation type
        var innerBindings = BuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames);
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

    static MemberBinding BuildSingleNavigationBinding(
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
        var innerBindings = BuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames);
        var memberInit = Expression.MemberInit(Expression.New(navType), innerBindings);

        // x.Parent == null ? null : new Parent { ... }
        var conditional = Expression.Condition(
            nullCheck,
            Expression.Constant(null, navType),
            memberInit);

        return Expression.Bind(prop, conditional);
    }

    static List<MemberBinding> BuildNavigationBindings(
        Expression sourceExpression,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var bindings = new List<MemberBinding>();
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
                prop.CanWrite &&
                addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(sourceExpression, prop)));
            }
        }

        // Add nested navigations recursively
        foreach (var (navFieldName, nestedNavProjection) in projection.Navigations)
        {
            if (TryGetProperty(entityType, navFieldName, out var prop) &&
                addedProperties.Add(prop.Name))
            {
                var binding = BuildNestedNavigationBinding(sourceExpression, prop, nestedNavProjection, keyNames);
                bindings.Add(binding);
            }
        }

        return bindings;
    }

    static MemberAssignment BuildNestedNavigationBinding(
        Expression sourceExpression,
        PropertyInfo prop,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;

        if (navProjection.IsCollection)
        {
            // sourceExpression.Children
            var navAccess = Expression.Property(sourceExpression, prop);
            var navParam = Expression.Parameter(navType, "n");

            // Build the inner MemberInit
            var innerBindings = BuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames);
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

            return Expression.Bind(prop, toListCall);
        }
        else
        {
            // sourceExpression.Parent
            var navAccess = Expression.Property(sourceExpression, prop);

            // sourceExpression.Parent == null
            var nullCheck = Expression.Equal(navAccess, Expression.Constant(null, navType));

            // Build the MemberInit
            var innerBindings = BuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames);
            var memberInit = Expression.MemberInit(Expression.New(navType), innerBindings);

            // sourceExpression.Parent == null ? null : new Parent { ... }
            var conditional = Expression.Condition(
                nullCheck,
                Expression.Constant(null, navType),
                memberInit);

            return Expression.Bind(prop, conditional);
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
