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

    static MethodInfo whereMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Where" &&
                    _.GetParameters().Length == 2 &&
                    _.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

    static FrozenDictionary<(string Name, bool Descending), MethodInfo> orderMethods = new Dictionary<(string, bool), MethodInfo>
    {
        [("OrderBy", false)] = orderByMethod,
        [("OrderBy", true)] = OrderMethod("OrderByDescending"),
        [("ThenBy", false)] = OrderMethod("ThenBy"),
        [("ThenBy", true)] = OrderMethod("ThenByDescending")
    }.ToFrozenDictionary();

    static MethodInfo OrderMethod(string name) =>
        enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == name &&
                        _.GetParameters().Length == 2);

    static ConcurrentDictionary<Type, MethodInfo> whereMethods = new();

    static ConcurrentDictionary<(MethodInfo Method, Type Item, Type Key), MethodInfo> genericMethods = new();

    static MethodInfo MakeGeneric(MethodInfo definition, Type itemType, Type keyType) =>
        genericMethods.GetOrAdd(
            (definition, itemType, keyType),
            _ => _.Method.MakeGenericMethod(_.Item, _.Key));

    static ConcurrentDictionary<Type, EntityTypeMetadata> entityMetadataCache = new();

    static IReadOnlyDictionary<Type, IReadOnlyList<Type>> noDerivedTypes = new Dictionary<Type, IReadOnlyList<Type>>();

    record PropertyMetadata(Type EntityType, PropertyInfo Property, bool CanWrite, bool IsAutoProperty)
    {
        /// <summary>
        /// Only the key property of a collection navigation ever needs this, but it was being
        /// constructed for every property of every entity type. MakeGenericMethod is not free.
        /// </summary>
        public MethodInfo OrderByMethod =>
            field ??= orderByMethod.MakeGenericMethod(EntityType, Property.PropertyType);
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
        where TEntity : class =>
        TryBuild(projection, keyNames, derivedTypes, out expression, out _);

    /// <param name="includePaths">
    /// The navigations under a navigation that was bound whole, because its type could not be
    /// projected. EF applies includes to entities in a projection, so these are added as includes
    /// alongside the select. Otherwise those deeper navigations were never loaded.
    /// </param>
    public static bool TryBuild<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out Expression<Func<TEntity, TEntity>>? expression,
        out IReadOnlyList<string> includePaths)
        where TEntity : class =>
        TryBuild(projection, keyNames, derivedTypes, out expression, out includePaths, out _);

    /// <param name="argumentFields">
    /// The navigation fields whose ids, where and orderBy were applied inside their collection
    /// subquery, so their resolvers know not to apply them again.
    /// </param>
    public static bool TryBuild<TEntity>(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes,
        [NotNullWhen(true)] out Expression<Func<TEntity, TEntity>>? expression,
        out IReadOnlyList<string> includePaths,
        out IReadOnlyList<GraphQLField> argumentFields)
        where TEntity : class
    {
        expression = null;
        var state = new BuildState(keyNames, derivedTypes);
        includePaths = state.IncludePaths;
        argumentFields = state.ArgumentFields;
        var entityType = typeof(TEntity);

        if (entityType.IsAbstract)
        {
            return false;
        }

        var parameter = GetEntityMetadata(entityType).Parameter;
        if (!TryBuildEntityInit(parameter, entityType, projection, state, null, out var body))
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
        BuildState state,
        string? path,
        [NotNullWhen(true)] out Expression? expression)
    {
        expression = null;

        if (!TryBuildMemberInit(source, entityType, projection, state, path, out var memberInit))
        {
            return false;
        }

        expression = memberInit;
        if (!state.DerivedTypes.TryGetValue(entityType, out var derived))
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
            if (!TryBuildMemberInit(derivedSource, derivedType, derivedProjection, state, path, out var derivedInit))
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
        BuildState state,
        string? path,
        [NotNullWhen(true)] out MemberInitExpression? memberInit)
    {
        memberInit = null;
        if (!TryBuildBindings(source, entityType, projection, state, path, out var bindings))
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
        BuildState state,
        string? path,
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
                var navPath = path is null ? metadata.Property.Name : $"{path}.{metadata.Property.Name}";
                if (!TryBuildNavigationBinding(navAccess, navProjection, state, navPath, out var binding))
                {
                    if (!metadata.CanWrite)
                    {
                        continue;
                    }

                    // Can't project navigation (e.g. read-only properties on target entity)
                    // Fall back to including the full navigation entity
                    binding = BuildFullNavigationBinding(navAccess, navProjection, state);
                    state.AddIncludePaths(navPath, navProjection.Projection);
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
        BuildState state,
        string path,
        [NotNullWhen(true)] out MemberAssignment? binding)
    {
        binding = null;
        var navType = navProjection.EntityType;

        // An abstract type cannot be created by a member init, and a navigation wanted whole has
        // no field list to build one from. Both fall back to binding the navigation itself.
        if (navType.IsAbstract || navProjection.IsWhole)
        {
            return false;
        }

        var navMetadata = GetEntityMetadata(navType);

        if (navProjection.IsCollection)
        {
            var navParam = Expression.Parameter(navType, "n");

            if (!TryBuildEntityInit(navParam, navType, navProjection.Projection, state, path, out var itemInit))
            {
                return false;
            }

            var itemLambda = Expression.Lambda(itemInit, navParam);

            var source = ApplyArguments(navAccess, navParam, navType, navProjection, state, true);

            // .Select(_ => new Child { ... }).ToList()
            var selectCall = Expression.Call(null, navMetadata.SelectMethod, source, itemLambda);
            var toListCall = Expression.Call(null, navMetadata.ToListMethod, selectCall);

            binding = Expression.Bind(navAccess.Member, toListCall);
            return true;
        }

        if (!TryBuildEntityInit(navAccess, navType, navProjection.Projection, state, path, out var init))
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
        NavigationProjectionInfo navProjection,
        BuildState state)
    {
        if (navProjection.IsCollection)
        {
            var navType = navProjection.EntityType;
            var navMetadata = GetEntityMetadata(navType);
            var navParam = Expression.Parameter(navType, "n");
            var source = ApplyArguments(navAccess, navParam, navType, navProjection, state, false);
            return Expression.Bind(navAccess.Member, Expression.Call(null, navMetadata.ToListMethod, source));
        }

        return Expression.Bind(navAccess.Member, navAccess);
    }

    /// <summary>
    /// The navigation field's ids, where and orderBy, applied to the collection inside the
    /// subquery so the database evaluates them against the table rather than the resolver
    /// evaluating them in memory against the projected columns. Without an orderBy the
    /// collection is ordered by key, where <paramref name="orderByKey"/>, for a deterministic result.
    /// </summary>
    static Expression ApplyArguments(
        Expression source,
        ParameterExpression navParam,
        Type navType,
        NavigationProjectionInfo navProjection,
        BuildState state,
        bool orderByKey)
    {
        var arguments = navProjection.Arguments;
        if (arguments is not null)
        {
            if (arguments.Ids is not null &&
                state.KeyNames.TryGetValue(navType, out var keyNames))
            {
                var keyName = ArgumentProcessor.GetKeyName(keyNames);
                source = Where(source, navParam, navType, ExpressionBuilder.BuildIdPredicate(navType, keyName, arguments.Ids));
            }

            if (arguments.Wheres.Count > 0)
            {
                source = Where(source, navParam, navType, ExpressionBuilder.BuildPredicate(navType, arguments.Wheres));
            }

            state.ArgumentFields.Add(arguments.Field);
        }

        if (arguments is { OrderBys.Count: > 0 })
        {
            var first = true;
            foreach (var orderBy in arguments.OrderBys)
            {
                var property = PropertyCache.GetProperty(navType, orderBy.Path);
                var key = new ParameterReplacer(property.SourceParameter, navParam).Visit(property.Left);
                var method = MakeGeneric(orderMethods[(first ? "OrderBy" : "ThenBy", orderBy.Descending)], navType, key.Type);
                source = Expression.Call(null, method, source, Expression.Lambda(key, navParam));
                first = false;
            }

            return source;
        }

        if (orderByKey &&
            state.KeyNames.TryGetValue(navType, out var keys) &&
            keys.Count > 0 &&
            GetEntityMetadata(navType).Properties.TryGetValue(keys[0], out var keyMetadata))
        {
            var keyAccess = Expression.Property(navParam, keyMetadata.Property);
            var keyLambda = Expression.Lambda(keyAccess, navParam);
            source = Expression.Call(null, keyMetadata.OrderByMethod, source, keyLambda);
        }

        return source;
    }

    static Expression Where(Expression source, ParameterExpression navParam, Type navType, LambdaExpression predicate)
    {
        // The predicate is built on the parameter the property cache shares for the type.
        // Rebind it to this subquery's own parameter.
        var body = new ParameterReplacer(predicate.Parameters[0], navParam).Visit(predicate.Body);
        var method = whereMethods.GetOrAdd(navType, _ => whereMethod.MakeGenericMethod(_));
        return Expression.Call(null, method, source, Expression.Lambda(body, navParam));
    }

    /// <summary>
    /// What one build shares across its recursion: the model lookups, and the include paths
    /// collected for navigations that were bound whole.
    /// </summary>
    sealed class BuildState(
        IReadOnlyDictionary<Type, List<string>> keyNames,
        IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes)
    {
        public IReadOnlyDictionary<Type, List<string>> KeyNames { get; } = keyNames;
        public IReadOnlyDictionary<Type, IReadOnlyList<Type>> DerivedTypes { get; } = derivedTypes;
        public List<string> IncludePaths { get; } = [];
        public List<GraphQLField> ArgumentFields { get; } = [];

        /// <summary>
        /// A navigation bound whole is materialized as a full entity, so everything requested
        /// under it has to arrive through includes. String include paths resolve navigations
        /// declared on derived types too, so the derived navigations need no cast.
        /// </summary>
        public void AddIncludePaths(string path, FieldProjectionInfo projection)
        {
            if (projection.Navigations is not null)
            {
                foreach (var (name, nested) in projection.Navigations)
                {
                    AddIncludePath(path, name, nested.Projection);
                }
            }

            if (projection.DerivedNavigations is not null)
            {
                foreach (var navigations in projection.DerivedNavigations.Values)
                {
                    foreach (var (name, nested) in navigations)
                    {
                        AddIncludePath(path, name, nested.Projection);
                    }
                }
            }
        }

        void AddIncludePath(string path, string name, FieldProjectionInfo projection)
        {
            var nestedPath = $"{path}.{name}";
            IncludePaths.Add(nestedPath);
            AddIncludePaths(nestedPath, projection);
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
