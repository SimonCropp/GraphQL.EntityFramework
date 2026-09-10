class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames,
    IReadOnlyDictionary<Type, IReadOnlySet<string>> foreignKeys,
    IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes)
{
    public bool TryGetProjectionExpressionWithFilters<TDbContext, TItem>(
        IResolveFieldContext context,
        Filters<TDbContext>? filters,
        [NotNullWhen(true)] out Expression<Func<TItem, TItem>>? expression)
        where TDbContext : DbContext
        where TItem : class
    {
        expression = null;

        if (context.SubFields is null)
        {
            return false;
        }

        var type = typeof(TItem);
        navigations.TryGetValue(type, out var navigationProperties);
        keyNames.TryGetValue(type, out var keys);
        foreignKeys.TryGetValue(type, out var fks);

        var projection = GetProjectionInfo(context, type, navigationProperties, keys, fks);

        if (filters is { HasFilters: true })
        {
            projection = MergeFilterFieldsIntoProjection(projection, filters, type);
        }

        return SelectExpressionBuilder.TryBuild(projection, keyNames, derivedTypes, out expression);
    }

    public IQueryable<TItem> AddIncludes<TDbContext, TItem>(
        IResolveFieldContext context,
        Filters<TDbContext>? filters,
        IQueryable<TItem> query)
        where TDbContext : DbContext
        where TItem : class
    {
        if (context.SubFields is null)
        {
            return query;
        }

        var type = typeof(TItem);
        navigations.TryGetValue(type, out var navigationProperties);
        keyNames.TryGetValue(type, out var keys);
        foreignKeys.TryGetValue(type, out var fks);

        var projection = GetProjectionInfo(context, type, navigationProperties, keys, fks);

        if (filters is { HasFilters: true })
        {
            projection = MergeFilterFieldsIntoProjection(projection, filters, type);
        }

        return AddIncludesFromProjection(query, projection);
    }

    static IQueryable<TItem> AddIncludesFromProjection<TItem>(
        IQueryable<TItem> query,
        FieldProjectionInfo projection)
        where TItem : class
    {
        var visitedTypes = new HashSet<Type> { typeof(TItem) };

        if (projection.Navigations is { Count: > 0 })
        {
            foreach (var (navName, navProjection) in projection.Navigations)
            {
                if (IsVisitedOrBaseType(navProjection.EntityType, visitedTypes))
                {
                    continue;
                }

                visitedTypes.Add(navProjection.EntityType);
                query = query.Include(navName);
                query = AddNestedIncludes(query, navName, navProjection.Projection, visitedTypes);
                visitedTypes.Remove(navProjection.EntityType);
            }
        }

        // Add derived-type navigation includes for TPH inline fragments
        // e.g. query.Include(e => ((GroupAccessRule)e).Group)
        if (projection.DerivedNavigations is { Count: > 0 })
        {
            query = AddDerivedTypeIncludes(query, projection.DerivedNavigations, visitedTypes);
        }

        return query;
    }

    static IQueryable<TItem> AddDerivedTypeIncludes<TItem>(
        IQueryable<TItem> query,
        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>> derivedNavigations,
        HashSet<Type> visitedTypes)
        where TItem : class
    {
        var itemType = typeof(TItem);
        var parameter = Expression.Parameter(itemType, "e");

        foreach (var (derivedType, navDict) in derivedNavigations)
        {
            // Cast: (DerivedType)e
            var cast = Expression.Convert(parameter, derivedType);

            foreach (var (navName, navProjection) in navDict)
            {
                if (IsVisitedOrBaseType(navProjection.EntityType, visitedTypes))
                {
                    continue;
                }

                // Property access: ((DerivedType)e).Navigation
                var property = derivedType.GetProperty(navName);
                if (property == null)
                {
                    continue;
                }

                var propertyAccess = Expression.Property(cast, property);

                // Build lambda: e => ((DerivedType)e).Navigation
                var lambda = Expression.Lambda(propertyAccess, parameter);

                // Call EntityFrameworkQueryableExtensions.Include(query, lambda)
                var includeMethod = GetIncludeMethod(itemType, property.PropertyType);
                query = (IQueryable<TItem>)includeMethod.Invoke(null, [query, lambda])!;
            }
        }

        return query;
    }

    static MethodInfo includeMethodDefinition = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Include" &&
                    _.GetGenericArguments().Length == 2 &&
                    _.GetParameters().Length == 2 &&
                    _.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

    static ConcurrentDictionary<(Type entity, Type property), MethodInfo> includeMethods = new();

    /// <summary>
    /// Both the scan over every public static method on EntityFrameworkQueryableExtensions and the
    /// MakeGenericMethod were being repeated per request. The pairs are bounded by the model, so
    /// they are safe to hold on to.
    /// </summary>
    static MethodInfo GetIncludeMethod(Type entityType, Type propertyType) =>
        includeMethods.GetOrAdd(
            (entityType, propertyType),
            _ => includeMethodDefinition.MakeGenericMethod(_.entity, _.property));

    static IQueryable<TItem> AddNestedIncludes<TItem>(
        IQueryable<TItem> query,
        string includePath,
        FieldProjectionInfo projection,
        HashSet<Type> visitedTypes)
        where TItem : class
    {
        if (projection.Navigations is not { Count: > 0 })
        {
            return query;
        }

        foreach (var (navName, navProjection) in projection.Navigations)
        {
            if (IsVisitedOrBaseType(navProjection.EntityType, visitedTypes))
            {
                continue;
            }

            visitedTypes.Add(navProjection.EntityType);
            var nestedPath = $"{includePath}.{navName}";
            query = query.Include(nestedPath);
            query = AddNestedIncludes(query, nestedPath, navProjection.Projection, visitedTypes);
            visitedTypes.Remove(navProjection.EntityType);
        }

        return query;
    }

    // Skip if the type was already visited OR if it's a base type of any visited type.
    // The latter prevents circular includes through TPH hierarchies where a navigation
    // points back to a base type (e.g. ParliamentaryAbsenceEmailAttachment.Request -> BaseRequest
    // when the root query is on TravelRequest which inherits from BaseRequest).
    static bool IsVisitedOrBaseType(Type entityType, HashSet<Type> visitedTypes) =>
        visitedTypes.Contains(entityType) ||
        visitedTypes.Any(entityType.IsAssignableFrom);

    FieldProjectionInfo MergeFilterFieldsIntoProjection<TDbContext>(
        FieldProjectionInfo projection,
        Filters<TDbContext> filters,
        Type entityType)
        where TDbContext : DbContext
    {
        navigations.TryGetValue(entityType, out var navigationProperties);

        foreach (var filter in filters.GetFiltersForHierarchy(entityType))
        {
            projection = filter.AddRequirements(projection, navigationProperties);
        }

        // Recursively process existing navigations
        if (projection.Navigations is not { Count: > 0 })
        {
            return projection;
        }

        var updatedNavigations = new Dictionary<string, NavigationProjectionInfo>(projection.Navigations);
        foreach (var (navName, navProjection) in projection.Navigations)
        {
            var updatedProjection = MergeFilterFieldsIntoProjection(navProjection.Projection, filters, navProjection.EntityType);
            if (updatedProjection != navProjection.Projection)
            {
                updatedNavigations[navName] = navProjection with { Projection = updatedProjection };
            }
        }

        return projection with { Navigations = updatedNavigations };
    }

    FieldProjectionInfo GetProjectionInfo(
        IResolveFieldContext context,
        Type entityType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (context.SubFields is not null)
        {
            foreach (var (field, fieldType) in context.SubFields.Values)
            {
                ProcessField(field, fieldType, navigationProperties, scalarFields, navProjections, context);
            }
        }

        // Scan for derived-type navigations from inline fragments (TPH support)
        var derivedNavigations = GetDerivedNavigationsFromFragments(context, entityType, scalarFields);

        return new(scalarFields, keys, foreignKeyNames, navProjections, derivedNavigations);
    }

    /// <summary>
    /// The field type carries the projection metadata, and its resolved graph type is what the
    /// selection set below it selects from. GraphQL.NET supplies both for the root sub fields
    /// only, so for nested selection sets they are recovered by walking the schema alongside
    /// the ast. Without that, a projection based field or a connection below the root was
    /// treated as a scalar named after the field, and nothing under it was projected.
    /// </summary>
    void ProcessField(
        GraphQLField field,
        FieldType? fieldType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // SubFields is keyed by response key, which is the alias when one is used.
        // The name of the property to project has to come from the ast node.
        var fieldName = field.Name.StringValue;

        if (IsConnectionNodeName(fieldName))
        {
            // edges, items and node are wrappers with no property of their own.
            // The entity fields are in the selection set below them.
            ProcessSelectionSet(field.SelectionSet, GetComplexGraphType(fieldType), navigationProperties, scalarFields, navProjections, context);
            return;
        }

        if (fieldType is not null &&
            TryGetProjectionMetadata(fieldType, out var projection))
        {
            ProcessProjectionExpression(field, fieldType, projection, navigationProperties, scalarFields, navProjections, context);
            return;
        }

        ProcessNavigationOrScalar(fieldName, field, fieldType, navigationProperties, scalarFields, navProjections, context);
    }

    void ProcessSelectionSet(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        if (selectionSet?.Selections is null)
        {
            return;
        }

        foreach (var (field, fieldGraphType) in EnumerateFields(selectionSet, graphType, context))
        {
            var fieldType = fieldGraphType?.GetField(field.Name.Value);
            ProcessField(field, fieldType, navigationProperties, scalarFields, navProjections, context);
        }
    }

    /// <summary>
    /// The graph type a field's selection set selects from. Null for a scalar or a field that could
    /// not be resolved, in which case the selection set is matched against the entity by name alone.
    /// </summary>
    static IComplexGraphType? GetComplexGraphType(FieldType? fieldType) =>
        fieldType?.ResolvedType?.GetNamedType() as IComplexGraphType;

    /// <summary>
    /// Navigations selected through a fragment on a type derived from the entity type. They do
    /// not exist on the entity type itself, so they are collected per derived type and included
    /// with a cast. Fragments on the entity type itself, or on one of its base types, select
    /// navigations the main projection already covers, so they are skipped.
    /// </summary>
    Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? GetDerivedNavigationsFromFragments(
        IResolveFieldContext context,
        Type entityType,
        HashSet<string> scalarFields)
    {
        var (selectionSet, leafGraphType) = GetLeafSelection(context);
        if (selectionSet?.Selections is null)
        {
            return null;
        }

        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? result = null;

        foreach (var (field, fieldGraphType) in EnumerateFields(selectionSet, leafGraphType, context))
        {
            if (fieldGraphType is null ||
                ReferenceEquals(fieldGraphType, leafGraphType) ||
                !TryFindDerivedClrType(fieldGraphType, out var derivedType) ||
                derivedType == entityType ||
                !entityType.IsAssignableFrom(derivedType))
            {
                continue;
            }

            navigations.TryGetValue(derivedType, out var derivedNavProps);
            if (derivedNavProps is null ||
                !derivedNavProps.TryGetValue(field.Name.StringValue, out var navigation))
            {
                // A scalar of the derived type. The root sub fields exclude fields conditional on
                // another type, so it is recorded here; it resolves against the derived type's
                // properties when that type's member init is built, and is skipped for the base.
                scalarFields.Add(field.Name.StringValue);
                continue;
            }

            result ??= [];
            if (!result.TryGetValue(derivedType, out var derivedNavs))
            {
                derivedNavs = [];
                result[derivedType] = derivedNavs;
            }

            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            var navGraphType = GetComplexGraphType(fieldGraphType.GetField(field.Name.Value));
            AddNavigation(
                derivedNavs,
                navigation.Name,
                new(
                    navType,
                    navigation.IsCollection,
                    GetNestedProjection(field.SelectionSet, navGraphType, nestedNavProps, nestedKeys, nestedFks, context)));
        }

        return result;
    }

    /// <summary>
    /// Navigate through connection wrapper fields (edges/items/node) to find the leaf selection set
    /// that contains the actual entity fields and inline fragments, and the graph type it selects from.
    /// </summary>
    static (GraphQLSelectionSet? SelectionSet, IComplexGraphType? GraphType) GetLeafSelection(IResolveFieldContext context)
    {
        var selectionSet = context.FieldAst.SelectionSet;
        if (selectionSet?.Selections is null)
        {
            return (null, null);
        }

        var graphType = GetComplexGraphType(context.FieldDefinition);

        // Drill through connection wrapper fields
        while (true)
        {
            var found = false;
            foreach (var selection in selectionSet.Selections)
            {
                if (selection is GraphQLField { SelectionSet: not null } field &&
                    IsConnectionNodeName(field.Name.StringValue))
                {
                    graphType = GetComplexGraphType(graphType?.GetField(field.Name.Value));
                    selectionSet = field.SelectionSet;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                break;
            }
        }

        return (selectionSet, graphType);
    }

    bool TryFindDerivedClrType(IGraphType graphType, [NotNullWhen(true)] out Type? clrType)
    {
        clrType = null;

        // Walk the type hierarchy to find the CLR type from the generic arguments
        var graphClrType = GetSourceType(graphType.GetType());
        if (graphClrType is not null && navigations.ContainsKey(graphClrType))
        {
            clrType = graphClrType;
            return true;
        }

        // Fallback: match CLR type name directly
        foreach (var type in navigations.Keys)
        {
            if (string.Equals(type.Name, graphType.Name, StringComparison.OrdinalIgnoreCase))
            {
                clrType = type;
                return true;
            }
        }

        return false;
    }

    static Type? GetSourceType(Type graphType)
    {
        var type = graphType;
        while (type is not null)
        {
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(ObjectGraphType<>) ||
                    genericDef == typeof(InterfaceGraphType<>))
                {
                    return type.GenericTypeArguments[0];
                }
            }

            type = type.BaseType;
        }

        return null;
    }

    static bool IsConnectionNodeName(string fieldName) =>
        fieldName.Equals("edges", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("items", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("node", StringComparison.OrdinalIgnoreCase);

    void ProcessProjectionExpression(
        GraphQLField field,
        FieldType fieldType,
        LambdaExpression projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        var accessedPaths = ProjectionAnalyzer.ExtractPropertyPaths(projection);

        // Group paths by their root navigation property
        var pathsByNavigation = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? primaryNavigation = null;

        foreach (var path in accessedPaths)
        {
            var dotIndex = path.IndexOf('.');
            var rootProperty = dotIndex >= 0 ? path[..dotIndex] : path;

            if (!pathsByNavigation.TryGetValue(rootProperty, out var paths))
            {
                paths = [];
                pathsByNavigation[rootProperty] = paths;
            }

            if (dotIndex >= 0)
            {
                paths.Add(path[(dotIndex + 1)..]);
            }

            primaryNavigation ??= rootProperty;
        }

        foreach (var (navName, nestedPaths) in pathsByNavigation)
        {
            if (!TryFindNavigation(navigationProperties, navName, out var navigation))
            {
                // Scalar field path (no navigation) — add root property to scalarFields
                scalarFields.Add(navName);
                continue;
            }

            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            FieldProjectionInfo nestedProjection;

            if (navName == primaryNavigation)
            {
                // Primary navigation: merge GraphQL fields with projection-required fields
                nestedProjection = GetNestedProjection(field.SelectionSet, GetComplexGraphType(fieldType), nestedNavProps, nestedKeys, nestedFks, context);
                foreach (var nestedPath in nestedPaths)
                {
                    if (!nestedPath.Contains('.'))
                    {
                        nestedProjection.ScalarFields.Add(nestedPath);
                    }
                }
            }
            else
            {
                // Secondary navigation: include only projection-required fields
                var nestedScalarFields = nestedPaths
                    .Where(_ => !_.Contains('.'))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                nestedProjection = new(nestedScalarFields, nestedKeys ?? [], nestedFks ?? new HashSet<string>(), []);
            }

            AddNavigation(navProjections, navigation.Name, new(navType, navigation.IsCollection, nestedProjection));
        }
    }

    static bool TryFindNavigation(
        IReadOnlyDictionary<string, Navigation>? properties,
        string name,
        [NotNullWhen(true)] out Navigation? navigation)
    {
        navigation = null;
        return properties is not null &&
               properties.TryGetValue(name, out navigation);
    }

    FieldProjectionInfo GetNestedProjection(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames,
        IResolveFieldContext context)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        ProcessSelectionSet(selectionSet, graphType, navigationProperties, scalarFields, navProjections, context);

        return new(scalarFields, keys, foreignKeyNames, navProjections);
    }

    /// <summary>
    /// The fields in a selection set, including those inside fragments at any depth. A fragment
    /// can name a type other than the parent's, so each field is paired with the graph type it
    /// selects from. Validation rejects fragment cycles, but they are guarded against anyway,
    /// since the recursion would otherwise never end.
    /// </summary>
    static IEnumerable<(GraphQLField Field, IComplexGraphType? GraphType)> EnumerateFields(
        GraphQLSelectionSet selectionSet,
        IComplexGraphType? graphType,
        IResolveFieldContext context,
        HashSet<string>? spreadsInProgress = null)
    {
        foreach (var selection in selectionSet.Selections)
        {
            switch (selection)
            {
                case GraphQLField field:
                    yield return (field, graphType);
                    break;
                case GraphQLInlineFragment inlineFragment:
                {
                    var fragmentGraphType = ResolveTypeCondition(inlineFragment.TypeCondition, graphType, context);
                    foreach (var item in EnumerateFields(inlineFragment.SelectionSet, fragmentGraphType, context, spreadsInProgress))
                    {
                        yield return item;
                    }

                    break;
                }
                case GraphQLFragmentSpread fragmentSpread:
                {
                    var name = fragmentSpread.FragmentName.Name;
                    var fragmentDefinition = context.Document.Definitions
                        .OfType<GraphQLFragmentDefinition>()
                        .SingleOrDefault(_ => _.FragmentName.Name == name);

                    if (fragmentDefinition?.SelectionSet.Selections is null)
                    {
                        break;
                    }

                    spreadsInProgress ??= [];
                    if (!spreadsInProgress.Add(name.StringValue))
                    {
                        break;
                    }

                    var fragmentGraphType = ResolveTypeCondition(fragmentDefinition.TypeCondition, graphType, context);
                    foreach (var item in EnumerateFields(fragmentDefinition.SelectionSet, fragmentGraphType, context, spreadsInProgress))
                    {
                        yield return item;
                    }

                    spreadsInProgress.Remove(name.StringValue);
                    break;
                }
            }
        }
    }

    static IComplexGraphType? ResolveTypeCondition(
        GraphQLTypeCondition? typeCondition,
        IComplexGraphType? parentGraphType,
        IResolveFieldContext context)
    {
        if (typeCondition is null)
        {
            return parentGraphType;
        }

        return context.Schema.AllTypes[typeCondition.Type.Name.StringValue] as IComplexGraphType ?? parentGraphType;
    }

    void ProcessNavigationOrScalar(
        string fieldName,
        GraphQLField field,
        FieldType? fieldType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        Navigation? navigation = null;
        navigationProperties?.TryGetValue(fieldName, out navigation);

        if (navigation == null)
        {
            scalarFields.Add(fieldName);
            return;
        }

        var navType = navigation.Type;
        navigations.TryGetValue(navType, out var nestedNavProps);
        keyNames.TryGetValue(navType, out var nestedKeys);
        foreignKeys.TryGetValue(navType, out var nestedFks);

        AddNavigation(
            navProjections,
            navigation.Name,
            new(
                navType,
                navigation.IsCollection,
                GetNestedProjection(field.SelectionSet, GetComplexGraphType(fieldType), nestedNavProps, nestedKeys, nestedFks, context)));
    }

    /// <summary>
    /// The same navigation can be selected more than once: under two aliases, through a fragment
    /// as well as directly, or by a projection based field alongside the navigation field itself.
    /// Each selection can ask for different fields, so they are merged rather than the first or
    /// the last one winning.
    /// </summary>
    static void AddNavigation(
        Dictionary<string, NavigationProjectionInfo> navProjections,
        string name,
        NavigationProjectionInfo navProjection) =>
        navProjections[name] = navProjections.TryGetValue(name, out var existing)
            ? existing.Merge(navProjection)
            : navProjection;

    public static void SetProjectionMetadata(FieldType fieldType, LambdaExpression projection) =>
        fieldType.Metadata["_EF_Projection"] = projection;

    static bool TryGetProjectionMetadata(FieldType fieldType, [NotNullWhen(true)] out LambdaExpression? projection)
    {
        if (fieldType.Metadata.TryGetValue("_EF_Projection", out var projectionObj))
        {
            projection = (LambdaExpression)projectionObj!;
            return true;
        }

        projection = null;
        return false;
    }
}
