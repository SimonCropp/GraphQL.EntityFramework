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

    static IReadOnlyDictionary<Type, IReadOnlyList<Type>> noDerivedTypes = new Dictionary<Type, IReadOnlyList<Type>>();

    record PropertyMetadata(Type EntityType, PropertyInfo Property, bool CanWrite, bool IsAutoProperty)
    {
        MethodInfo? cachedOrderBy;

        /// <summary>
        /// Only the key property of a collection navigation ever needs this, but it was being
        /// constructed for every property of every entity type. MakeGenericMethod is not free.
        /// </summary>
        public MethodInfo OrderByMethod =>
            cachedOrderBy ??= orderByMethod.MakeGenericMethod(EntityType, Property.PropertyType);
    }

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
        where TEntity : class =>
        TryBuild(projection, keyNames, noDerivedTypes, out expression);

    /// <param name="derivedTypes">
    /// For each entity type that has derived types in the model, those derived types, base most
    /// first. A member init always creates the type it names, so projecting such a type as a plain
    /// member init would materialize every row as the base type and lose the derived identity.
    /// These are instead projected as a chain of type tests, each creating the matching type.
    /// </param>
    public static bool TryBuild<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out Expression<Func<TEntity, TEntity>>? expression)
        where TEntity : class
    {
        expression = null;
        var entityType = typeof(TEntity);

        if (entityType.IsAbstract)
        {
            return false;
        }

        var parameter = GetEntityMetadata(entityType).Parameter;
        if (!TryBuildEntityInit(parameter, entityType, projection, keyNames, derivedTypes, out var body))
        {
            return false;
        }

        expression = Expression.Lambda<Func<TEntity, TEntity>>(body, parameter);
        return true;
    }

    /// <summary>
    /// The expression that creates <paramref name="entityType"/> from <paramref name="source"/>:
    /// a member init, or for a type with derived types a chain of type tests each creating the
    /// matching type, most derived first. False when the entity, or one of the navigations under
    /// it that has no writable property to fall back to, cannot be projected.
    /// </summary>
    static bool TryBuildEntityInit(
        Expression source,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out Expression? expression)
    {
        expression = null;

        if (!TryBuildMemberInit(source, entityType, projection, keyNames, derivedTypes, out var memberInit))
        {
            return false;
        }

        expression = memberInit;
        if (!derivedTypes.TryGetValue(entityType, out var derived))
        {
            return true;
        }

        // Wrapping base most first leaves the most derived test outermost, so it runs first
        foreach (var derivedType in derived)
        {
            if (derivedType.IsAbstract)
            {
                continue;
            }

            var derivedProjection = ProjectionForDerivedType(projection, derivedType);
            var derivedSource = Expression.Convert(source, derivedType);
            if (!TryBuildMemberInit(derivedSource, derivedType, derivedProjection, keyNames, derivedTypes, out var derivedInit))
            {
                return false;
            }

            expression = Expression.Condition(
                Expression.TypeIs(source, derivedType),
                Expression.Convert(derivedInit, entityType),
                expression);
        }

        return true;
    }

    /// <summary>
    /// The fields selected through fragments on a derived type are recorded against the base
    /// projection: scalars by name, which resolve against the derived type's properties, and
    /// navigations under <see cref="FieldProjectionInfo.DerivedNavigations"/>.
    /// </summary>
    static FieldProjectionInfo ProjectionForDerivedType(FieldProjectionInfo projection, Type derivedType)
    {
        if (projection.DerivedNavigations is null ||
            !projection.DerivedNavigations.TryGetValue(derivedType, out var derivedNavigations))
        {
            return projection;
        }

        return projection.Merge(new([], null, null, derivedNavigations));
    }

    static bool TryBuildMemberInit(
        Expression source,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out MemberInitExpression? memberInit)
    {
        memberInit = null;
        if (!TryBuildBindings(source, entityType, projection, keyNames, derivedTypes, out var bindings))
        {
            return false;
        }

        memberInit = Expression.MemberInit(GetEntityMetadata(entityType).NewInstance, bindings);
        return true;
    }

    static bool TryBuildBindings(
        Expression source,
        Type entityType,
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out List<MemberBinding>? bindings)
    {
        // Pre-size collections to avoid reallocations
        var capacity = (projection.KeyNames?.Count ?? 0) + (projection.ForeignKeyNames?.Count ?? 0) + projection.ScalarFields.Count + (projection.Navigations?.Count ?? 0);
        bindings = [with(capacity)];
        var addedProperties = new HashSet<string>(capacity, StringComparer.OrdinalIgnoreCase);
        var properties = GetEntityMetadata(entityType).Properties;

        // Add key properties
        if (projection.KeyNames != null)
        {
            foreach (var keyName in projection.KeyNames)
            {
                if (properties.TryGetValue(keyName, out var metadata) &&
                    addedProperties.Add(keyName))
                {
                    if (!metadata.CanWrite || !metadata.IsAutoProperty)
                    {
                        // Key property has a custom setter that may throw during materialization,
                        // or is read-only. Can't use projection.
                        bindings = null;
                        return false;
                    }

                    bindings.Add(Bind(source, metadata));
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
                    bindings.Add(Bind(source, metadata));
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

                bindings.Add(Bind(source, metadata));
            }
        }

        // Add nested navigations recursively
        if (projection.Navigations != null)
        {
            foreach (var (navFieldName, navProjection) in projection.Navigations)
            {
                if (!properties.TryGetValue(navFieldName, out var metadata) ||
                    !addedProperties.Add(navFieldName))
                {
                    continue;
                }

                var navAccess = Expression.Property(source, metadata.Property);
                if (!TryBuildNavigationBinding(navAccess, navProjection, keyNames, derivedTypes, out var binding))
                {
                    if (!metadata.CanWrite)
                    {
                        continue;
                    }

                    // Can't project navigation (e.g. read-only properties on target entity)
                    // Fall back to including the full navigation entity
                    binding = BuildFullNavigationBinding(navAccess, navProjection);
                }

                bindings.Add(binding);
            }
        }

        Sort(bindings);

        return true;
    }

    static MemberAssignment Bind(Expression source, PropertyMetadata metadata) =>
        Expression.Bind(metadata.Property, Expression.Property(source, metadata.Property));

    /// <summary>
    /// Bindings are collected in the order the fields were requested, so the same set of fields
    /// asked for in a different order produced a structurally different expression tree, and
    /// therefore a separate entry in EF's compiled query cache for identical work. Ordering by
    /// member name makes the tree depend only on which fields were requested.
    /// </summary>
    static void Sort(List<MemberBinding> bindings) =>
        bindings.Sort((x, y) => string.CompareOrdinal(x.Member.Name, y.Member.Name));

    static bool TryBuildNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out MemberAssignment? binding)
    {
        binding = null;
        var navType = navProjection.EntityType;

        if (navType.IsAbstract)
        {
            return false;
        }

        var navMetadata = GetEntityMetadata(navType);

        if (navProjection.IsCollection)
        {
            var navParam = Expression.Parameter(navType, "n");

            if (!TryBuildEntityInit(navParam, navType, navProjection.Projection, keyNames, derivedTypes, out var itemInit))
            {
                return false;
            }

            var itemLambda = Expression.Lambda(itemInit, navParam);

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

            // .Select(_ => new Child { ... }).ToList()
            var selectCall = Expression.Call(null, navMetadata.SelectMethod, orderedCollection, itemLambda);
            var toListCall = Expression.Call(null, navMetadata.ToListMethod, selectCall);

            binding = Expression.Bind(navAccess.Member, toListCall);
            return true;
        }

        if (!TryBuildEntityInit(navAccess, navType, navProjection.Projection, keyNames, derivedTypes, out var init))
        {
            return false;
        }

        // source.Parent == null ? null : new Parent { ... }
        var conditional = Expression.Condition(
            Expression.Equal(navAccess, navMetadata.NullConstant),
            navMetadata.NullConstant,
            init);

        binding = Expression.Bind(navAccess.Member, conditional);
        return true;
    }

    static MemberAssignment BuildFullNavigationBinding(
        MemberExpression navAccess,
        NavigationProjectionInfo navProjection)
    {
        if (navProjection.IsCollection)
        {
            var navMetadata = GetEntityMetadata(navProjection.EntityType);
            return Expression.Bind(navAccess.Member, Expression.Call(null, navMetadata.ToListMethod, navAccess));
        }

        return Expression.Bind(navAccess.Member, navAccess);
    }

    static EntityTypeMetadata GetEntityMetadata(Type type) =>
        entityMetadataCache.GetOrAdd(type, type =>
        {
            var parameter = Expression.Parameter(type, "x");
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dictionary = new Dictionary<string, PropertyMetadata>(properties.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var property in properties)
            {
                var canWrite = property.CanWrite;
                var isAutoProperty = canWrite &&
                                     property.SetMethod!.IsDefined(typeof(CompilerGeneratedAttribute), false);
                dictionary[property.Name] = new(type, property, canWrite, isAutoProperty);
            }

            var newInstance = Expression.New(type);
            var selectMethod = SelectExpressionBuilder.selectMethod.MakeGenericMethod(type, type);
            var toListMethod = SelectExpressionBuilder.toListMethod.MakeGenericMethod(type);
            var nullConstant = Expression.Constant(null, type);

            return new(parameter, dictionary, newInstance, selectMethod, toListMethod, nullConstant);
        });
}
