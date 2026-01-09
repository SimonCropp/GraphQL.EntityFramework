namespace GraphQL.EntityFramework;

static class SelectExpressionBuilder
{
    static Type enumerableType = typeof(Enumerable);

    static MethodInfo orderByMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "OrderBy" &&
                    _.GetParameters().Length == 2);

    static MethodInfo selectMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Select" &&
                    _.GetParameters().Length == 2);

    static MethodInfo toListMethod = enumerableType
        .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!;

    static ConcurrentDictionary<string, object> cache = new();
    static ConcurrentDictionary<Type, EntityTypeMetadata> entityMetadataCache = new();

    record PropertyMetadata(PropertyInfo Property, bool CanWrite, MemberExpression PropertyAccess, MemberBinding? Binding, MethodInfo OrderByMethod);

    record EntityTypeMetadata(
        ParameterExpression Parameter,
        IReadOnlyDictionary<string, PropertyMetadata> Properties,
        NewExpression NewInstance,
        MethodInfo SelectMethod,
        MethodInfo ToListMethod);

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
        var entityMetadata = GetEntityMetadata(entityType);
        var parameter = entityMetadata.Parameter;
        var properties = entityMetadata.Properties;
        var bindings = new List<MemberBinding>();
        var addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Always include key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (properties.TryGetValue(keyName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(keyName))
            {
                bindings.Add(metadata.Binding!);
            }
        }

        // 2. Always include foreign key properties
        foreach (var fkName in projection.ForeignKeyNames)
        {
            if (properties.TryGetValue(fkName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(fkName))
            {
                bindings.Add(metadata.Binding!);
            }
        }

        // 3. Add requested scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (properties.TryGetValue(fieldName, out var metadata) &&
                addedProperties.Add(fieldName))
            {
                if (!metadata.CanWrite)
                {
                    // Read-only property (expression-bodied or database computed column)
                    // Can't use projection - return null to load full entity
                    return null;
                }

                bindings.Add(metadata.Binding!);
            }
        }

        // 4. Add navigation properties with nested projections
        foreach (var (navFieldName, navProjection) in projection.Navigations)
        {
            if (!properties.TryGetValue(navFieldName, out var metadata) ||
                !addedProperties.Add(navFieldName))
            {
                continue;
            }

            var binding = BuildNavigationBinding(metadata.PropertyAccess, navProjection, keyNames);
            if (binding == null)
            {
                // Can't project navigation - return null to load full entity
                return null;
            }

            bindings.Add(binding);
        }

        var memberInit = Expression.MemberInit(entityMetadata.NewInstance, bindings);
        return Expression.Lambda<Func<TEntity, TEntity>>(memberInit, parameter);
    }

    static MemberBinding? BuildNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames) =>
        navProjection.IsCollection
            ? BuildCollectionNavigationBinding(navAccess, navProjection, keyNames)
            : BuildSingleNavigationBinding(navAccess, navProjection, keyNames);

    static MemberAssignment? BuildCollectionNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;

        // Can't create NewExpression for abstract types
        if (navType.IsAbstract)
        {
            return null;
        }

        var navMetadata = GetEntityMetadata(navType);
        var navParam = Expression.Parameter(navType, "n");

        // Build the inner MemberInit for the navigation type
        if (!TryBuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames, out var innerBindings))
        {
            // Can't project navigation - return null to load full entity
            return null;
        }

        var innerMemberInit = Expression.MemberInit(navMetadata.NewInstance, innerBindings);
        var innerLambda = Expression.Lambda(innerMemberInit, navParam);

        // Build: x.Children.OrderBy(n => n.Key) to ensure deterministic ordering
        Expression orderedCollection = navAccess;
        if (keyNames.TryGetValue(navType, out var keys) && keys.Count > 0)
        {
            if (navMetadata.Properties.TryGetValue(keys[0], out var keyMetadata))
            {
                var keyAccess = Expression.Property(navParam, keyMetadata.Property);
                var keyLambda = Expression.Lambda(keyAccess, navParam);

                orderedCollection = Expression.Call(null, keyMetadata.OrderByMethod, navAccess, keyLambda);
            }
        }

        // Build: x.Children.OrderBy(...).Select(n => new Child { ... })
        var selectCall = Expression.Call(null, navMetadata.SelectMethod, orderedCollection, innerLambda);

        // Build: .ToList()
        var toListCall = Expression.Call(null, navMetadata.ToListMethod, selectCall);

        return Expression.Bind(navAccess.Member, toListCall);
    }

    static MemberBinding? BuildSingleNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;

        // Can't create NewExpression for abstract types
        if (navType.IsAbstract)
        {
            return null;
        }

        var navMetadata = GetEntityMetadata(navType);

        // x.Parent == null
        var nullCheck = Expression.Equal(navAccess, Expression.Constant(null, navType));

        // Build the MemberInit for the navigation type using navAccess as source
        if (!TryBuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames, out var innerBindings))
        {
            // Can't project navigation - return null to load full entity
            return null;
        }

        var memberInit = Expression.MemberInit(navMetadata.NewInstance, innerBindings);

        // x.Parent == null ? null : new Parent { ... }
        var conditional = Expression.Condition(
            nullCheck,
            Expression.Constant(null, navType),
            memberInit);

        return Expression.Bind(navAccess.Member, conditional);
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
        var properties = GetEntityMetadata(entityType).Properties;

        // Add key properties
        foreach (var keyName in projection.KeyNames)
        {
            if (properties.TryGetValue(keyName, out var metadata) &&
                metadata.CanWrite &&
                addedProperties.Add(keyName))
            {
                bindings.Add(Expression.Bind(metadata.Property, Expression.Property(sourceExpression, metadata.Property)));
            }
        }

        // Add scalar properties
        foreach (var fieldName in projection.ScalarFields)
        {
            if (properties.TryGetValue(fieldName, out var metadata) &&
                addedProperties.Add(fieldName))
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
            if (!properties.TryGetValue(navFieldName, out var metadata) ||
                !addedProperties.Add(navFieldName))
            {
                continue;
            }

            if (!TryBuildNestedNavigationBinding(sourceExpression, metadata.Property, nestedNavProjection, keyNames, out var binding))
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
        PropertyInfo property,
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
            var navMetadata = GetEntityMetadata(navType);
            var navParam = Expression.Parameter(navType, "n");

            // sourceExpression.Children
            var navAccess = Expression.Property(sourceExpression, property);

            // Build the inner MemberInit
            if (!TryBuildNavigationBindings(navParam, navType, navProjection.Projection, keyNames, out var innerBindings))
            {
                // Can't project navigation - return false to load full entity
                return false;
            }

            var innerMemberInit = Expression.MemberInit(navMetadata.NewInstance, innerBindings);
            var innerLambda = Expression.Lambda(innerMemberInit, navParam);

            // .OrderBy(o => o.Key) to ensure deterministic ordering
            Expression orderedCollection = navAccess;
            if (keyNames.TryGetValue(navType, out var keys) && keys.Count > 0)
            {
                if (navMetadata.Properties.TryGetValue(keys[0], out var keyMetadata))
                {
                    var keyAccess = Expression.Property(navParam, keyMetadata.Property);
                    var keyLambda = Expression.Lambda(keyAccess, navParam);

                    orderedCollection = Expression.Call(null, keyMetadata.OrderByMethod, navAccess, keyLambda);
                }
            }

            // .Select(n => new Child { ... })
            var selectCall = Expression.Call(null, navMetadata.SelectMethod, orderedCollection, innerLambda);

            // .ToList()
            var toListCall = Expression.Call(null, navMetadata.ToListMethod, selectCall);

            binding = Expression.Bind(property, toListCall);
            return true;
        }
        else
        {
            var navMetadata = GetEntityMetadata(navType);

            // sourceExpression.Parent
            var navAccess = Expression.Property(sourceExpression, property);

            // sourceExpression.Parent == null
            var nullCheck = Expression.Equal(navAccess, Expression.Constant(null, navType));

            // Build the MemberInit
            if (!TryBuildNavigationBindings(navAccess, navType, navProjection.Projection, keyNames, out var innerBindings))
            {
                // Can't project navigation - return false to load full entity
                return false;
            }

            var memberInit = Expression.MemberInit(navMetadata.NewInstance, innerBindings);

            // sourceExpression.Parent == null ? null : new Parent { ... }
            var conditional = Expression.Condition(
                nullCheck,
                Expression.Constant(null, navType),
                memberInit);

            binding = Expression.Bind(property, conditional);
            return true;
        }
    }

    static EntityTypeMetadata GetEntityMetadata(Type type) =>
        entityMetadataCache.GetOrAdd(type, type =>
        {
            var parameter = Expression.Parameter(type, "x");
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dictionary = new Dictionary<string, PropertyMetadata>(properties.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var property in properties)
            {
                var propertyAccess = Expression.Property(parameter, property);
                var binding = property.CanWrite ? Expression.Bind(property, propertyAccess) : null;
                var orderByMethod = SelectExpressionBuilder.orderByMethod.MakeGenericMethod(type, property.PropertyType);
                dictionary[property.Name] = new(property, property.CanWrite, propertyAccess, binding, orderByMethod);
            }

            var newInstance = Expression.New(type);
            var selectMethod = SelectExpressionBuilder.selectMethod.MakeGenericMethod(type, type);
            var toListMethod = SelectExpressionBuilder.toListMethod.MakeGenericMethod(type);

            return new(parameter, dictionary, newInstance, selectMethod, toListMethod);
        });

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
