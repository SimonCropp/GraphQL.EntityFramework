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
            var prop = GetProperty(entityType, keyName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 2. Add requested scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            var prop = GetProperty(entityType, fieldName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(parameter, prop)));
            }
        }

        // 3. Add navigation properties with nested projections
        foreach (var (navFieldName, navProjection) in projection.Navigations)
        {
            var prop = GetProperty(entityType, navFieldName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                var binding = BuildNavigationBinding(parameter, prop, navProjection, keyNames);
                if (binding != null)
                {
                    bindings.Add(binding);
                }
            }
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

    static MemberBinding BuildCollectionNavigationBinding(
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
            var keyProp = GetProperty(navType, keys[0]);
            if (keyProp != null)
            {
                var keyAccess = Expression.Property(orderParam, keyProp);
                var keyLambda = Expression.Lambda(keyAccess, orderParam);

                var orderByMethod = typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .First(m => m.Name == "OrderBy" &&
                                m.GetParameters().Length == 2)
                    .MakeGenericMethod(navType, keyProp.PropertyType);

                orderedCollection = Expression.Call(null, orderByMethod, navAccess, keyLambda);
            }
        }

        // Build: x.Children.OrderBy(...).Select(n => new Child { ... })
        var selectMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Select" &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
            .MakeGenericMethod(navType, navType);

        var selectCall = Expression.Call(null, selectMethod, orderedCollection, innerLambda);

        // Build: .ToList()
        var toListMethod = typeof(Enumerable)
            .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!
            .MakeGenericMethod(navType);

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
            var prop = GetProperty(entityType, keyName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(sourceExpression, prop)));
            }
        }

        // Add scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            var prop = GetProperty(entityType, fieldName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                bindings.Add(Expression.Bind(prop, Expression.Property(sourceExpression, prop)));
            }
        }

        // Add nested navigations recursively
        foreach (var (navFieldName, nestedNavProjection) in projection.Navigations)
        {
            var prop = GetProperty(entityType, navFieldName);
            if (prop != null && addedProperties.Add(prop.Name))
            {
                var binding = BuildNestedNavigationBinding(sourceExpression, prop, nestedNavProjection, keyNames);
                if (binding != null)
                {
                    bindings.Add(binding);
                }
            }
        }

        return bindings;
    }

    static MemberBinding? BuildNestedNavigationBinding(
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
                var keyProp = GetProperty(navType, keys[0]);
                if (keyProp != null)
                {
                    var keyAccess = Expression.Property(orderParam, keyProp);
                    var keyLambda = Expression.Lambda(keyAccess, orderParam);

                    var orderByMethod = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m => m.Name == "OrderBy" &&
                                    m.GetParameters().Length == 2)
                        .MakeGenericMethod(navType, keyProp.PropertyType);

                    orderedCollection = Expression.Call(null, orderByMethod, navAccess, keyLambda);
                }
            }

            // .Select(n => new Child { ... })
            var selectMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "Select" &&
                            m.GetParameters().Length == 2 &&
                            m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(navType, navType);

            var selectCall = Expression.Call(null, selectMethod, orderedCollection, innerLambda);

            // .ToList()
            var toListMethod = typeof(Enumerable)
                .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(navType);

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

    static PropertyInfo? GetProperty(Type type, string name) =>
        type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

    static string BuildCacheKey<TEntity>(FieldProjectionInfo projection)
    {
        var sb = new StringBuilder();
        sb.Append(typeof(TEntity).FullName);
        sb.Append('|');
        BuildProjectionKey(sb, projection);
        return sb.ToString();
    }

    static void BuildProjectionKey(StringBuilder sb, FieldProjectionInfo projection)
    {
        // Sort scalar fields for consistent cache key
        var sortedScalars = projection.ScalarFields.OrderBy(_ => _, StringComparer.OrdinalIgnoreCase);
        sb.Append(string.Join(",", sortedScalars));

        if (projection.Navigations.Count > 0)
        {
            sb.Append('{');
            var sortedNavs = projection.Navigations.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var (navName, navProjection) in sortedNavs)
            {
                sb.Append(navName);
                sb.Append(':');
                BuildProjectionKey(sb, navProjection.Projection);
                sb.Append(';');
            }
            sb.Append('}');
        }
    }
}
