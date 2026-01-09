namespace GraphQL.EntityFramework;

static class SelectExpressionBuilder
{
    static readonly ConcurrentDictionary<string, object> cache = new();
    static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyMetadata>> propertyCache = new();

    record PropertyMetadata(PropertyInfo Property, bool CanWrite);

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

    static Expression<Func<TEntity, TEntity>>? BuildExpression<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");
        var bindings = new List<MemberBinding>();
        var addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var properties = GetPropertiesForType(entityType);

        // 1. Always include key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (properties.TryGetValue(keyName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(metadata.Property.Name))
            {
                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(parameter, metadata.Property)));
            }
        }

        // 2. Always include foreign key properties
        foreach (var fkName in projection.ForeignKeyNames)
        {
            if (properties.TryGetValue(fkName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(metadata.Property.Name))
            {
                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(parameter, metadata.Property)));
            }
        }

        // 3. Add requested scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (properties.TryGetValue(fieldName, out var metadata) &&
                addedProperties.Add(metadata.Property.Name))
            {
                if (!metadata.CanWrite)
                {
                    // Read-only property (expression-bodied or database computed column)
                    // Can't use projection - return null to load full entity
                    return null;
                }

                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(parameter, metadata.Property)));
            }
        }

        // 4. Add navigation properties with nested projections
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

        // Can't create NewExpression for abstract types
        if (navType.IsAbstract)
        {
            return null;
        }

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

        // Can't create NewExpression for abstract types
        if (navType.IsAbstract)
        {
            return null;
        }

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
        var properties = GetPropertiesForType(entityType);

        // Add key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (properties.TryGetValue(keyName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(metadata.Property.Name))
            {
                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(sourceExpression, metadata.Property)));
            }
        }

        // Add scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (properties.TryGetValue(fieldName, out var metadata) &&
                addedProperties.Add(metadata.Property.Name))
            {
                if (!metadata.CanWrite)
                {
                    // Read-only property (expression-bodied or database computed column)
                    // Can't use projection - return false to load full entity
                    bindings = null;
                    return false;
                }

                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(sourceExpression, metadata.Property)));
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

        // Can't create NewExpression for abstract types
        if (navType.IsAbstract)
        {
            return false;
        }

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

    static IReadOnlyDictionary<string, PropertyMetadata> GetPropertiesForType(Type type) =>
        propertyCache.GetOrAdd(type, t =>
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dict = new Dictionary<string, PropertyMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in properties)
            {
                dict[prop.Name] = new(prop, prop.CanWrite);
            }

            return dict;
        });

    static bool TryGetProperty(Type type, string name, [NotNullWhen(true)] out PropertyInfo? property)
    {
        var properties = GetPropertiesForType(type);
        if (properties.TryGetValue(name, out var metadata))
        {
            property = metadata.Property;
            return true;
        }

        property = null;
        return false;
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
