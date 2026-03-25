using System.Reflection;

class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames,
    IReadOnlyDictionary<Type, IReadOnlySet<string>> foreignKeys)
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

        var projection = GetProjectionInfo(context, navigationProperties, keys, fks);

        if (filters is { HasFilters: true })
        {
            projection = MergeFilterFieldsIntoProjection(projection, filters, type);
        }

        return SelectExpressionBuilder.TryBuild(projection, keyNames, out expression);
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

        var projection = GetProjectionInfo(context, navigationProperties, keys, fks);

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

    static MethodInfo GetIncludeMethod(Type entityType, Type propertyType) =>
        typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "Include" &&
                        _.GetGenericArguments().Length == 2 &&
                        _.GetParameters().Length == 2 &&
                        _.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .MakeGenericMethod(entityType, propertyType);

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

        foreach (var filter in filters.GetFilters(entityType))
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
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (context.SubFields is not null)
        {
            foreach (var (fieldName, fieldInfo) in context.SubFields)
            {
                if (IsConnectionNodeName(fieldName))
                {
                    ProcessConnectionNodeFields(fieldInfo.Field.SelectionSet, navigationProperties, scalarFields, navProjections, context);
                }
                else
                {
                    ProcessProjectionField(fieldName, fieldInfo, navigationProperties, scalarFields, navProjections, context);
                }
            }
        }

        // Scan for derived-type navigations from inline fragments (TPH support)
        var derivedNavigations = GetDerivedNavigationsFromFragments(context);

        return new(scalarFields, keys, foreignKeyNames, navProjections, derivedNavigations);
    }

    Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? GetDerivedNavigationsFromFragments(
        IResolveFieldContext context)
    {
        var selectionSet = GetLeafSelectionSet(context);
        if (selectionSet?.Selections is null)
        {
            return null;
        }

        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? result = null;

        foreach (var selection in selectionSet.Selections)
        {
            GraphQLTypeCondition? typeCondition;
            GraphQLSelectionSet? fragmentSelectionSet;

            switch (selection)
            {
                case GraphQLInlineFragment inlineFragment:
                    typeCondition = inlineFragment.TypeCondition;
                    fragmentSelectionSet = inlineFragment.SelectionSet;
                    break;
                case GraphQLFragmentSpread fragmentSpread:
                {
                    var name = fragmentSpread.FragmentName.Name;
                    var fragmentDefinition = context.Document.Definitions
                        .OfType<GraphQLFragmentDefinition>()
                        .SingleOrDefault(_ => _.FragmentName.Name == name);
                    if (fragmentDefinition is null)
                    {
                        continue;
                    }

                    typeCondition = fragmentDefinition.TypeCondition;
                    fragmentSelectionSet = fragmentDefinition.SelectionSet;
                    break;
                }
                default:
                    continue;
            }

            if (typeCondition is null || fragmentSelectionSet?.Selections is null)
            {
                continue;
            }

            var typeName = typeCondition.Type.Name.StringValue;

            // Find the CLR type for this GraphQL type name using the schema
            if (!TryFindDerivedClrType(typeName, context.Schema, out var derivedType))
            {
                continue;
            }

            // Get navigation properties for this derived type
            if (!navigations.TryGetValue(derivedType, out var derivedNavProps))
            {
                continue;
            }

            // Process fields in this fragment against the derived type's navigation properties
            foreach (var field in fragmentSelectionSet.Selections.OfType<GraphQLField>())
            {
                var fieldName = field.Name.StringValue;
                if (!derivedNavProps.TryGetValue(fieldName, out var navigation))
                {
                    continue;
                }

                result ??= [];
                if (!result.TryGetValue(derivedType, out var derivedNavs))
                {
                    derivedNavs = [];
                    result[derivedType] = derivedNavs;
                }

                if (derivedNavs.ContainsKey(navigation.Name))
                {
                    continue;
                }

                var navType = navigation.Type;
                navigations.TryGetValue(navType, out var nestedNavProps);
                keyNames.TryGetValue(navType, out var nestedKeys);
                foreignKeys.TryGetValue(navType, out var nestedFks);

                derivedNavs[navigation.Name] = new(
                    navType,
                    navigation.IsCollection,
                    GetNestedProjection(field.SelectionSet, nestedNavProps, nestedKeys, nestedFks, context));
            }
        }

        return result;
    }

    /// <summary>
    /// Navigate through connection wrapper fields (edges/items/node) to find the leaf selection set
    /// that contains the actual entity fields and inline fragments.
    /// </summary>
    static GraphQLSelectionSet? GetLeafSelectionSet(IResolveFieldContext context)
    {
        var selectionSet = context.FieldAst.SelectionSet;
        if (selectionSet?.Selections is null)
        {
            return null;
        }

        // Drill through connection wrapper fields
        while (true)
        {
            var found = false;
            foreach (var selection in selectionSet.Selections)
            {
                if (selection is GraphQLField field && IsConnectionNodeName(field.Name.StringValue))
                {
                    if (field.SelectionSet is not null)
                    {
                        selectionSet = field.SelectionSet;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                break;
            }
        }

        return selectionSet;
    }

    bool TryFindDerivedClrType(string graphQlTypeName, ISchema schema, [NotNullWhen(true)] out Type? clrType)
    {
        clrType = null;

        // Use the schema's type lookup to resolve GraphQL type name → CLR type
        var graphType = schema.AllTypes.FirstOrDefault(_ => _.Name == graphQlTypeName);
        if (graphType is not null)
        {
            // Walk the type hierarchy to find the CLR type from the generic arguments
            var graphClrType = GetSourceType(graphType.GetType());
            if (graphClrType is not null && navigations.ContainsKey(graphClrType))
            {
                clrType = graphClrType;
                return true;
            }
        }

        // Fallback: match CLR type name directly
        foreach (var type in navigations.Keys)
        {
            if (string.Equals(type.Name, graphQlTypeName, StringComparison.OrdinalIgnoreCase))
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

    void ProcessConnectionNodeFields(
        GraphQLSelectionSet? selectionSet,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        if (selectionSet?.Selections is null)
        {
            return;
        }

        foreach (var selection in EnumerateFields(selectionSet, context))
        {
            var fieldName = selection.Name.StringValue;
            if (IsConnectionNodeName(fieldName))
            {
                ProcessConnectionNodeFields(selection.SelectionSet, navigationProperties, scalarFields, navProjections, context);
            }
            else
            {
                ProcessNestedProjectionField(fieldName, selection, navigationProperties, scalarFields, navProjections, context);
            }
        }
    }

    static bool IsConnectionNodeName(string fieldName) =>
        fieldName.Equals("edges", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("items", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("node", StringComparison.OrdinalIgnoreCase);

    void ProcessProjectionField(
        string fieldName,
        (GraphQLField Field, FieldType FieldType) fieldInfo,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        if (TryGetProjectionMetadata(fieldInfo.FieldType, out var projection))
        {
            ProcessProjectionExpression(fieldInfo, projection, navigationProperties, scalarFields, navProjections, context);
            return;
        }

        ProcessNestedProjectionField(fieldName, fieldInfo.Field, navigationProperties, scalarFields, navProjections, context);
    }

    void ProcessProjectionExpression(
        (GraphQLField Field, FieldType FieldType) fieldInfo,
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
            if (!TryFindNavigation(navigationProperties, navName, out var navigation, out var actualNavName))
            {
                // Scalar field path (no navigation) — add root property to scalarFields
                scalarFields.Add(navName);
                continue;
            }

            if (navProjections.ContainsKey(actualNavName))
            {
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
                nestedProjection = GetNestedProjection(fieldInfo.Field.SelectionSet, nestedNavProps, nestedKeys, nestedFks, context);
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

            navProjections[actualNavName] = new(navType, navigation.IsCollection, nestedProjection);
        }
    }

    static bool TryFindNavigation(
        IReadOnlyDictionary<string, Navigation>? properties,
        string name,
        [NotNullWhen(true)] out Navigation? navigation,
        [NotNullWhen(true)] out string? actualName)
    {
        navigation = null;
        actualName = null;

        if (properties == null)
        {
            return false;
        }

        if (properties.TryGetValue(name, out navigation))
        {
            actualName = name;
            return true;
        }

        foreach (var (key, value) in properties)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                navigation = value;
                actualName = key;
                return true;
            }
        }

        return false;
    }

    FieldProjectionInfo GetNestedProjection(
        GraphQLSelectionSet? selectionSet,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames,
        IResolveFieldContext context)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (selectionSet?.Selections is null)
        {
            return new(scalarFields, keys, foreignKeyNames, navProjections);
        }

        foreach (var field in EnumerateFields(selectionSet, context))
        {
            ProcessNestedProjectionField(field.Name.StringValue, field, navigationProperties, scalarFields, navProjections, context);
        }

        return new(scalarFields, keys, foreignKeyNames, navProjections);
    }

    static IEnumerable<GraphQLField> EnumerateFields(GraphQLSelectionSet selectionSet, IResolveFieldContext context)
    {
        foreach (var selection in selectionSet.Selections)
        {
            switch (selection)
            {
                case GraphQLField field:
                    yield return field;
                    break;
                case GraphQLInlineFragment inlineFragment:
                    foreach (var field in inlineFragment.SelectionSet.Selections.OfType<GraphQLField>())
                    {
                        yield return field;
                    }

                    break;
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

                    foreach (var field in fragmentDefinition.SelectionSet.Selections.OfType<GraphQLField>())
                    {
                        yield return field;
                    }

                    break;
                }
            }
        }
    }

    void ProcessNestedProjectionField(
        string fieldName,
        GraphQLField field,
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

        navProjections[navigation.Name] = new(
            navType,
            navigation.IsCollection,
            GetNestedProjection(field.SelectionSet, nestedNavProps, nestedKeys, nestedFks, context));
    }

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
