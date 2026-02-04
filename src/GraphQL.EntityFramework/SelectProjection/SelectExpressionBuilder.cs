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

    static ConcurrentDictionary<Type, EntityTypeMetadata> entityMetadataCache = new();

    record PropertyMetadata(PropertyInfo Property, bool CanWrite, MemberExpression PropertyAccess, MemberBinding? Binding, MethodInfo OrderByMethod);

    record EntityTypeMetadata(
        ParameterExpression Parameter,
        IReadOnlyDictionary<string, PropertyMetadata> Properties,
        NewExpression NewInstance,
        MethodInfo SelectMethod,
        MethodInfo ToListMethod,
        ConstantExpression NullConstant);

    public static bool TryBuild<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out Expression<Func<TEntity, TEntity>>? expression)
        where TEntity : class
    {
        expression = BuildExpression<TEntity>(projection, keyNames);
        return expression != null;
    }

    static Expression<Func<TEntity, TEntity>>? BuildExpression<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
        where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (entityType.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Cannot project abstract type '{entityType.Name}'. Either change the type to be concrete, or use .OfType<ConcreteType>() to query specific derived types.");
        }

        var entityMetadata = GetEntityMetadata(entityType);
        var parameter = entityMetadata.Parameter;
        var properties = entityMetadata.Properties;

        // Pre-size collections to avoid reallocations
        var capacity = projection.KeyNames?.Count ?? 0 + projection.ForeignKeyNames?.Count ?? 0 +
                      projection.ScalarFields.Count + projection.Navigations?.Count ?? 0;
        var bindings = new List<MemberBinding>(capacity);
        var addedProperties = new HashSet<string>(capacity, StringComparer.OrdinalIgnoreCase);

        // 1. Always include key properties
        if (projection.KeyNames != null)
        {
            foreach (var keyName in projection.KeyNames)
            {
                if (properties.TryGetValue(keyName, out var metadata) &&
                    metadata.CanWrite &&
                    addedProperties.Add(keyName))
                {
                    bindings.Add(metadata.Binding!);
                }
            }
        }

        // 2. Always include foreign key properties
        if (projection.ForeignKeyNames != null)
        {
            foreach (var fkName in projection.ForeignKeyNames)
            {
                if (properties.TryGetValue(fkName, out var metadata) &&
                    metadata.CanWrite &&
                    addedProperties.Add(fkName))
                {
                    bindings.Add(metadata.Binding!);
                }
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
        if (projection.Navigations != null)
        {
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
                    if (!metadata.CanWrite)
                    {
                        continue;
                    }

                    // Can't project navigation (e.g. read-only properties on target entity)
                    // Fall back to including the full navigation entity
                    binding = BuildFullNavigationBinding(metadata, navProjection);
                }

                bindings.Add(binding);
            }
        }

        var memberInit = Expression.MemberInit(entityMetadata.NewInstance, bindings);
        return Expression.Lambda<Func<TEntity, TEntity>>(memberInit, parameter);
    }

    static MemberBinding? BuildNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        if (navProjection.EntityType.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Cannot project abstract navigation type '{navProjection.EntityType.Name}'. Either change the type to be concrete, or use an explicit projection to extract the required properties.");
        }

        return navProjection.IsCollection
            ? BuildCollectionNavigationBinding(navAccess, navProjection, keyNames)
            : BuildSingleNavigationBinding(navAccess, navProjection, keyNames);
    }

    static MemberAssignment? BuildCollectionNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames)
    {
        var navType = navProjection.EntityType;
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

        // Build: x.Children.OrderBy(_ => _.Key) to ensure deterministic ordering
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
        var navMetadata = GetEntityMetadata(navType);

        // x.Parent == null
        var nullCheck = Expression.Equal(navAccess, navMetadata.NullConstant);

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
            navMetadata.NullConstant,
            memberInit);

        return Expression.Bind(navAccess.Member, conditional);
    }

    static MemberBinding BuildFullNavigationBinding(
        PropertyMetadata metadata,
        NavigationProjectionInfo navProjection)
    {
        if (navProjection.IsCollection)
        {
            var navMetadata = GetEntityMetadata(navProjection.EntityType);
            return Expression.Bind(metadata.Property, Expression.Call(null, navMetadata.ToListMethod, metadata.PropertyAccess));
        }

        return Expression.Bind(metadata.Property, metadata.PropertyAccess);
    }

    static MemberAssignment BuildFullNestedNavigationBinding(
        PropertyInfo property,
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection)
    {
        if (navProjection.IsCollection)
        {
            var navMetadata = GetEntityMetadata(navProjection.EntityType);
            return Expression.Bind(property, Expression.Call(null, navMetadata.ToListMethod, navAccess));
        }

        return Expression.Bind(property, navAccess);
    }

    static bool TryBuildNavigationBindings(
        Expression sourceExpression,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        [NotNullWhen(true)] out List<MemberBinding>? bindings)
    {
        // Pre-size collections to avoid reallocations
        var capacity = projection.KeyNames?.Count ?? 0 + projection.ForeignKeyNames?.Count ?? 0 + projection.ScalarFields.Count + projection.Navigations?.Count ?? 0;
        bindings = new(capacity);
        var addedProperties = new HashSet<string>(capacity, StringComparer.OrdinalIgnoreCase);
        var properties = GetEntityMetadata(entityType).Properties;

        // Add key properties
        if (projection.KeyNames != null)
        {
            foreach (var keyName in projection.KeyNames)
            {
                if (properties.TryGetValue(keyName, out var metadata) &&
                    metadata.CanWrite &&
                    addedProperties.Add(keyName))
                {
                    bindings.Add(Expression.Bind(metadata.Property, Expression.Property(sourceExpression, metadata.Property)));
                }
            }
        }

        // Add foreign key properties
        if (projection.ForeignKeyNames != null)
        {
            foreach (var fkName in projection.ForeignKeyNames)
            {
                if (properties.TryGetValue(fkName, out var metadata) &&
                    metadata.CanWrite &&
                    addedProperties.Add(fkName))
                {
                    bindings.Add(Expression.Bind(metadata.Property, Expression.Property(sourceExpression, metadata.Property)));
                }
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
        if (projection.Navigations != null)
        {
            foreach (var (navFieldName, nestedNavProjection) in projection.Navigations)
            {
                if (!properties.TryGetValue(navFieldName, out var metadata) ||
                    !addedProperties.Add(navFieldName))
                {
                    continue;
                }

                if (!TryBuildNestedNavigationBinding(sourceExpression, metadata.Property, nestedNavProjection, keyNames, out var binding))
                {
                    if (!metadata.CanWrite)
                    {
                        continue;
                    }

                    // Can't project nested navigation (e.g. read-only properties on target entity)
                    // Fall back to including the full navigation entity
                    var navAccess = Expression.Property(sourceExpression, metadata.Property);
                    binding = BuildFullNestedNavigationBinding(metadata.Property, navAccess, nestedNavProjection);
                }

                bindings.Add(binding);
            }
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

        if (navType.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Cannot project abstract navigation type '{navType.Name}'. Either change the type to be concrete, or use an explicit projection to extract the required properties.");
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

            // .OrderBy(_ => _.Key) to ensure deterministic ordering
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

            // .Select(_ => new Child { ... })
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
            var nullCheck = Expression.Equal(navAccess, navMetadata.NullConstant);

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
                navMetadata.NullConstant,
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
            var nullConstant = Expression.Constant(null, type);

            return new(parameter, dictionary, newInstance, selectMethod, toListMethod, nullConstant);
        });
}
