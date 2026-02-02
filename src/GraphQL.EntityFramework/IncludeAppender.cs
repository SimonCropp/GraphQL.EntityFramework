class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames,
    IReadOnlyDictionary<Type, IReadOnlySet<string>> foreignKeys)
{
    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class
    {
        if (context.SubFields is null)
        {
            return query;
        }

        var type = typeof(TItem);
        if (!navigations.TryGetValue(type, out var navigationProperty))
        {
            return query;
        }

        return AddIncludes(query, context, navigationProperty);
    }

    public IQueryable<TItem> AddIncludesWithFilters<TItem>(
        IQueryable<TItem> query,
        IResolveFieldContext context,
        IReadOnlyDictionary<Type, IReadOnlySet<string>>? allFilterFields)
        where TItem : class =>
        AddIncludesWithFiltersAndDetectNavigations(query, context, allFilterFields);

    internal IQueryable<TItem> AddIncludesWithFiltersAndDetectNavigations<TItem>(
        IQueryable<TItem> query,
        IResolveFieldContext context,
        IReadOnlyDictionary<Type, IReadOnlySet<string>>? allFilterFields)
        where TItem : class
    {
        // First add includes from GraphQL query
        query = AddIncludes(query, context);

        // Then add includes for filter-required navigations
        if (allFilterFields is { Count: > 0 })
        {
            var type = typeof(TItem);
            if (navigations.TryGetValue(type, out var navigationProperties))
            {
                query = AddFilterIncludes(query, allFilterFields, type, navigationProperties);
            }
        }

        return query;
    }

    static IQueryable<TItem> AddFilterIncludes<TItem>(
        IQueryable<TItem> query,
        IReadOnlyDictionary<Type, IReadOnlySet<string>> allFilterFields,
        Type entityType,
        IReadOnlyDictionary<string, Navigation> navigationProperties)
        where TItem : class
    {
        // Get filter fields for this entity type and its base types
        var relevantFilterFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (filterType, filterFields) in allFilterFields)
        {
            if (filterType.IsAssignableFrom(entityType))
            {
                foreach (var field in filterFields)
                {
                    relevantFilterFields.Add(field);
                }
            }
        }

        if (relevantFilterFields.Count == 0)
        {
            return query;
        }

        // Extract navigation names from filter fields (e.g., "TravelRequest.GroupOwnerId" -> "TravelRequest")
        var filterNavigations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in relevantFilterFields)
        {
            if (field.Contains('.'))
            {
                var navName = field.Split('.', 2)[0];
                filterNavigations.Add(navName);
            }
        }

        // Note: Abstract filter navigations are now prevented by FilterEntry validation
        // All filter-required navigations here should be from explicit projections
        // that extract specific properties (not identity projections)
        // These are handled via the projection system in TryGetProjectionExpressionWithFilters
        // which only selects the required columns, not all columns

        return query;
    }

    public FieldProjectionInfo? GetProjection<TItem>(IResolveFieldContext context)
        where TItem : class
    {
        if (context.SubFields is null)
        {
            return null;
        }

        var type = typeof(TItem);
        navigations.TryGetValue(type, out var navigationProperties);
        keyNames.TryGetValue(type, out var keys);
        foreignKeys.TryGetValue(type, out var fks);

        return GetProjectionInfo(context, navigationProperties, keys, fks);
    }

    public bool TryGetProjectionExpressionWithFilters<TItem>(
        IResolveFieldContext context,
        IReadOnlyDictionary<Type, IReadOnlySet<string>>? allFilterFields,
        [NotNullWhen(true)] out Expression<Func<TItem, TItem>>? expression)
        where TItem : class
    {
        expression = null;
        var projection = GetProjection<TItem>(context);

        if (projection == null)
        {
            return false;
        }

        // Merge filter fields if provided (recursively for navigations)
        if (allFilterFields is { Count: > 0 })
        {
            projection = MergeFilterFieldsIntoProjection(projection, allFilterFields, typeof(TItem));
        }

        return SelectExpressionBuilder.TryBuild(projection, keyNames, out expression);
    }

    FieldProjectionInfo MergeFilterFieldsIntoProjection(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<Type, IReadOnlySet<string>> allFilterFields,
        Type entityType)
    {
        // Get filter fields for this entity type and its base types
        var relevantFilterFields = new List<string>();
        foreach (var (filterType, filterFields) in allFilterFields)
        {
            if (filterType.IsAssignableFrom(entityType))
            {
                relevantFilterFields.AddRange(filterFields);
            }
        }

        if (relevantFilterFields.Count == 0)
        {
            return projection;
        }

        // Get navigation metadata for this entity type
        navigations.TryGetValue(entityType, out var navigationProperties);

        // Separate simple fields and navigation paths
        var scalarFieldsToAdd = new List<string>();
        var navigationPaths = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in relevantFilterFields)
        {
            if (field.Contains('.'))
            {
                // Navigation path like "Parent.Id"
                var parts = field.Split('.', 2);
                var navName = parts[0];
                var navProperty = parts[1];

                if (!navigationPaths.TryGetValue(navName, out var properties))
                {
                    properties = new(StringComparer.OrdinalIgnoreCase);
                    navigationPaths[navName] = properties;
                }

                // Only add if it doesn't contain further dots (single-level navigation)
                if (!navProperty.Contains('.'))
                {
                    properties.Add(navProperty);
                }
            }
            else
            {
                // Simple field - check if it's a navigation property
                var isNavigation = navigationProperties?.ContainsKey(field) == true;

                if (isNavigation ||
                    projection.ScalarFields.Contains(field) ||
                    projection.KeyNames?.Contains(field, StringComparer.OrdinalIgnoreCase) == true)
                {
                    // Skip navigation names - they'll be handled via navigation paths
                    continue;
                }

                scalarFieldsToAdd.Add(field);
            }
        }

        // Merge scalar fields
        var mergedScalars = new HashSet<string>(projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
        foreach (var field in scalarFieldsToAdd) mergedScalars.Add(field);

        // Merge navigations
        var infos = projection.Navigations;
        Dictionary<string, NavigationProjectionInfo> mergedNavigations;
        if (infos == null)
        {
            mergedNavigations = [];
        }
        else
        {
            mergedNavigations = new(infos);
        }


        // Process navigation paths from filter fields
        foreach (var (navName, requiredProps) in navigationPaths)
        {
            // Skip if no navigation metadata available for this entity type
            if (navigationProperties == null)
            {
                continue;
            }

            // Try to find the navigation - use case-insensitive search
            Navigation? navMetadata = null;
            foreach (var (key, value) in navigationProperties)
            {
                if (string.Equals(key, navName, StringComparison.OrdinalIgnoreCase))
                {
                    navMetadata = value;
                    break;
                }
            }

            if (navMetadata == null)
            {
                continue;
            }

            var navType = navMetadata.Type;

            if (mergedNavigations.TryGetValue(navName, out var existingNav))
            {
                // Navigation exists in GraphQL query - add filter-required properties to its projection
                var updatedScalars = new HashSet<string>(existingNav.Projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
                foreach (var prop in requiredProps)
                {
                    updatedScalars.Add(prop);
                }

                var updatedProjection = existingNav.Projection with
                {
                    ScalarFields = updatedScalars
                };
                updatedProjection = MergeFilterFieldsIntoProjection(updatedProjection, allFilterFields, navType);
                mergedNavigations[navName] = existingNav with
                {
                    Projection = updatedProjection
                };
            }
            else
            {
                // Create navigation projection for filter-only navigations
                // Note: For abstract types, we still create the projection here
                // If SelectExpressionBuilder can't handle it, TryBuild will return false
                // and the Include added by AddFilterIncludes will be used instead
                var navProjection = new FieldProjectionInfo(requiredProps, null, null, null);
                navProjection = MergeFilterFieldsIntoProjection(navProjection, allFilterFields, navType);
                mergedNavigations[navName] = new(navType, navMetadata.IsCollection, navProjection);
            }
        }

        // Recursively process existing navigations
        if (projection.Navigations != null) foreach (var (navName, navProjection) in projection.Navigations)
        {
            if (!mergedNavigations.ContainsKey(navName))
            {
                var updated = MergeFilterFieldsIntoProjection(navProjection.Projection, allFilterFields, navProjection.EntityType);
                mergedNavigations[navName] = navProjection with
                {
                    Projection = updated
                };
            }
        }

        return projection
            with
            {
                ScalarFields = mergedScalars,
                Navigations = mergedNavigations
            };
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
                // Handle connection wrapper fields (edges, items, node)
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

        return new(scalarFields, keys, foreignKeyNames, navProjections);
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

        foreach (var selection in selectionSet.Selections.OfType<GraphQLField>())
        {
            var fieldName = selection.Name.StringValue;

            // Recursively handle nested connection nodes (e.g., edges -> node)
            if (IsConnectionNodeName(fieldName))
            {
                ProcessConnectionNodeFields(selection.SelectionSet, navigationProperties, scalarFields, navProjections, context);
            }
            else
            {
                // Process as regular field
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
        // Check if this field has a projection expression (new approach - flows to Select)
        if (TryGetProjectionMetadata(fieldInfo.FieldType, out var projection, out var sourceType))
        {
            var countBefore = navProjections.Count;
            ProcessProjectionExpression(
                fieldInfo,
                projection,
                sourceType,
                navigationProperties,
                navProjections,
                context);
            // Only return if we added navigations; otherwise fall through to include metadata
            // This handles cases like abstract types where projection can't be built
            if (navProjections.Count > countBefore)
            {
                return;
            }
        }

        // Check if this field has include metadata (fallback for abstract types, or legacy/obsolete approach)
        if (TryGetIncludeMetadata(fieldInfo.FieldType, out var includeNames))
        {
            // It's a navigation field - include ALL navigation properties from metadata
            var addedAny = false;
            foreach (var navName in includeNames)
            {
                Navigation? navigation = null;
                navigationProperties?.TryGetValue(navName, out navigation);

                if (navigation != null && !navProjections.ContainsKey(navigation.Name))
                {
                    var navType = navigation.Type;
                    navigations.TryGetValue(navType, out var nestedNavProps);
                    keyNames.TryGetValue(navType, out var nestedKeys);
                    foreignKeys.TryGetValue(navType, out var nestedFks);

                    // Only the first (primary) navigation gets the nested projection from the query
                    // Other navigations get empty projections (select all their keys and foreign keys)
                    var nestedProjection = addedAny
                        ? new([], nestedKeys ?? [], nestedFks ?? new HashSet<string>(), [])
                        : GetNestedProjection(
                            fieldInfo.Field.SelectionSet,
                            nestedNavProps,
                            nestedKeys,
                            nestedFks,
                            context);

                    navProjections[navigation.Name] = new(
                        navType,
                        navigation.IsCollection,
                        nestedProjection);
                    addedAny = true;
                }
            }

            if (addedAny)
            {
                return;
            }
        }

        // Check if this field is a navigation property by name
        Navigation? navByName = null;
        navigationProperties?.TryGetValue(fieldName, out navByName);

        if (navByName != null)
        {
            // It's a navigation - build nested projection
            var navType = navByName.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            var nestedProjection = GetNestedProjection(
                fieldInfo.Field.SelectionSet,
                nestedNavProps,
                nestedKeys,
                nestedFks,
                context);

            navProjections[navByName.Name] = new(
                navType,
                navByName.IsCollection,
                nestedProjection);
        }
        else
        {
            // It's a scalar field
            scalarFields.Add(fieldName);
        }
    }

    /// <summary>
    /// Processes a projection expression to build NavigationProjectionInfo entries.
    /// The expression is analyzed to determine which navigations and properties to include
    /// in the Select projection.
    /// </summary>
    void ProcessProjectionExpression(
        (GraphQLField Field, FieldType FieldType) fieldInfo,
        LambdaExpression projection,
        Type sourceType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Extract property paths from the projection expression
        var accessedPaths = ProjectionPathAnalyzer.ExtractPropertyPaths(projection, sourceType);

        // Group paths by their root navigation property
        var pathsByNavigation = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var primaryNavigation = (string?)null;

        foreach (var path in accessedPaths)
        {
            var rootProperty = path.Contains('.') ? path[..path.IndexOf('.')] : path;

            if (!pathsByNavigation.TryGetValue(rootProperty, out var paths))
            {
                paths = [];
                pathsByNavigation[rootProperty] = paths;
            }

            // Store the nested path (without root) if it's a nested access
            if (path.Contains('.'))
            {
                paths.Add(path[(path.IndexOf('.') + 1)..]);
            }

            // First navigation encountered is the primary one (gets GraphQL fields)
            primaryNavigation ??= rootProperty;
        }

        // Build NavigationProjectionInfo for each accessed navigation
        foreach (var (navName, nestedPaths) in pathsByNavigation)
        {
            // Try case-sensitive first, then case-insensitive
            Navigation? navigation = null;
            string? actualNavName = null;
            if (navigationProperties != null)
            {
                if (navigationProperties.TryGetValue(navName, out navigation))
                {
                    actualNavName = navName;
                }
                else
                {
                    // Case-insensitive fallback
                    var match = navigationProperties.FirstOrDefault(kvp =>
                        string.Equals(kvp.Key, navName, StringComparison.OrdinalIgnoreCase));
                    if (match.Value != null)
                    {
                        navigation = match.Value;
                        actualNavName = match.Key;
                    }
                }
            }

            if (navigation == null || actualNavName == null || navProjections.ContainsKey(actualNavName))
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
                nestedProjection = GetNestedProjection(
                    fieldInfo.Field.SelectionSet,
                    nestedNavProps,
                    nestedKeys,
                    nestedFks,
                    context);

                // Add any specific fields from the projection expression
                foreach (var nestedPath in nestedPaths)
                {
                    if (!nestedPath.Contains('.') &&
                        !nestedProjection.ScalarFields.Contains(nestedPath))
                    {
                        nestedProjection.ScalarFields.Add(nestedPath);
                    }
                }
            }
            else
            {
                // Secondary navigation: include only projection-required fields
                var scalarFields = nestedPaths
                    .Where(p => !p.Contains('.'))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                nestedProjection = new(scalarFields, nestedKeys ?? [], nestedFks ?? new HashSet<string>(), []);
            }

            navProjections[actualNavName] = new(
                navType,
                navigation.IsCollection,
                nestedProjection);
        }
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

        // Process direct fields
        foreach (var selection in selectionSet.Selections.OfType<GraphQLField>())
        {
            var fieldName = selection.Name.StringValue;
            ProcessNestedProjectionField(fieldName, selection, navigationProperties, scalarFields, navProjections, context);
        }

        // Process inline fragments
        foreach (var inlineFragment in selectionSet.Selections.OfType<GraphQLInlineFragment>())
        {
            foreach (var selection in inlineFragment.SelectionSet.Selections.OfType<GraphQLField>())
            {
                var fieldName = selection.Name.StringValue;
                ProcessNestedProjectionField(fieldName, selection, navigationProperties, scalarFields, navProjections, context);
            }
        }

        // Process fragment spreads
        foreach (var fragmentSpread in selectionSet.Selections.OfType<GraphQLFragmentSpread>())
        {
            var name = fragmentSpread.FragmentName.Name;
            var fragmentDefinition = context.Document.Definitions
                .OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(_ => _.FragmentName.Name == name);

            if (fragmentDefinition?.SelectionSet.Selections is null)
            {
                continue;
            }

            foreach (var selection in fragmentDefinition.SelectionSet.Selections.OfType<GraphQLField>())
            {
                var fieldName = selection.Name.StringValue;
                ProcessNestedProjectionField(fieldName, selection, navigationProperties, scalarFields, navProjections, context);
            }
        }

        return new(scalarFields, keys, foreignKeyNames, navProjections);
    }

    void ProcessNestedProjectionField(
        string fieldName,
        GraphQLField field,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Check if this field is a navigation property
        Navigation? navigation = null;
        navigationProperties?.TryGetValue(fieldName, out navigation);

        if (navigation == null)
        {
            // It's a scalar field - avoid duplicates
            scalarFields.Add(fieldName);
        }
        else
        {
            // It's a navigation - build nested projection
            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            var nestedProjection = GetNestedProjection(
                field.SelectionSet,
                nestedNavProps,
                nestedKeys,
                nestedFks,
                context);

            navProjections[navigation.Name] = new(
                navType,
                navigation.IsCollection,
                nestedProjection);
        }
    }

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, IResolveFieldContext context, IReadOnlyDictionary<string, Navigation> navigationProperties)
        where T : class
    {
        var paths = GetPaths(context, navigationProperties);
        return paths.Aggregate(query, (current, path) => current.Include(path));
    }

    List<string> GetPaths(IResolveFieldContext context, IReadOnlyDictionary<string, Navigation> navigationProperty)
    {
        var list = new List<string>();

        AddField(list, context.FieldAst, context.FieldAst.SelectionSet!, null, context.FieldDefinition, navigationProperty, context);

        return list;
    }

    void AddField(List<string> list, GraphQLField field, GraphQLSelectionSet selectionSet, string? parentPath, FieldType fieldType, IReadOnlyDictionary<string, Navigation> parentNavigationProperties, IResolveFieldContext context, IComplexGraphType? graph = null)
    {
        if (graph == null &&
            !fieldType.TryGetComplexGraph(out graph))
        {
            return;
        }

        var subFields = selectionSet.Selections.OfType<GraphQLField>().ToList();

        foreach (var inlineFragment in selectionSet.Selections.OfType<GraphQLInlineFragment>())
        {
            if (inlineFragment.TypeCondition!.Type.GraphTypeFromType(context.Schema) is IComplexGraphType graphFragment)
            {
                AddField(list, field, inlineFragment.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graphFragment);
            }
        }

        foreach (var fragmentSpread in selectionSet.Selections.OfType<GraphQLFragmentSpread>())
        {
            var name = fragmentSpread.FragmentName.Name;
            var fragmentDefinition = context.Document.Definitions
                .OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(_ => _.FragmentName.Name == name);
            if (fragmentDefinition is null)
            {
                continue;
            }

            AddField(list, field, fragmentDefinition.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graph);
        }

        if (IsConnectionNode(field) || field == context.FieldAst)
        {
            if (subFields.Count > 0)
            {
                ProcessSubFields(list, parentPath, subFields, graph, parentNavigationProperties, context);
            }

            return;
        }

        if (!fieldType.TryGetEntityTypeForField(out var entityType))
        {
            return;
        }

        // Check if entity type has navigation properties BEFORE processing includes
        // Scalar types (enums, primitives) won't be in the navigations dictionary
        // and shouldn't have includes added - projection handles loading scalar data
        if (!navigations.TryGetValue(entityType, out var navigationProperties))
        {
            return;
        }

        if (!TryGetIncludeMetadata(fieldType, out var includeNames))
        {
            return;
        }

        var paths = GetPaths(parentPath, includeNames).ToList();
        list.AddRange(paths);

        ProcessSubFields(list, paths.First(), subFields, graph, navigationProperties, context);
    }

    static IEnumerable<string> GetPaths(string? parentPath, string[] includeNames)
    {
        if (parentPath is null)
        {
            return includeNames;
        }

        return includeNames.Select(includeName => $"{parentPath}.{includeName}");
    }

    void ProcessSubFields(List<string> list, string? parentPath, List<GraphQLField> subFields, IComplexGraphType graph, IReadOnlyDictionary<string, Navigation> navigationProperties, IResolveFieldContext context)
    {
        foreach (var subField in subFields)
        {
            var single = graph.Fields.SingleOrDefault(_ => _.Name == subField.Name);
            if (single is not null)
            {
                AddField(list, subField, subField.SelectionSet!, parentPath, single, navigationProperties, context);
            }
        }
    }

    static bool IsConnectionNode(GraphQLField field)
    {
        var name = field.Name.StringValue.ToLowerInvariant();
        return name is "edges" or "items" or "node";
    }

    public static void SetIncludeMetadata(FieldType fieldType, string fieldName, IEnumerable<string>? includeNames) =>
        SetIncludeMetadata(fieldName, includeNames, fieldType.Metadata);

    static void SetIncludeMetadata(string fieldName, IEnumerable<string>? includeNames, IDictionary<string, object?> metadata)
    {
        if (includeNames is null)
        {
            metadata["_EF_IncludeName"] = FieldNameToArray(fieldName);
        }
        else
        {
            metadata["_EF_IncludeName"] = includeNames.ToArray();
        }
    }

    static string[] FieldNameToArray(string fieldName) =>
        [char.ToUpperInvariant(fieldName[0]) + fieldName[1..]];

    static bool TryGetIncludeMetadata(FieldType fieldType, [NotNullWhen(true)] out string[]? value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[]) fieldNameObject!;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Stores a projection expression in field metadata.
    /// This is used by navigation fields to specify which properties to project,
    /// flowing through to the root Select expression.
    /// </summary>
    public static void SetProjectionMetadata(FieldType fieldType, LambdaExpression projection, Type sourceType)
    {
        fieldType.Metadata["_EF_Projection"] = projection;
        fieldType.Metadata["_EF_ProjectionSourceType"] = sourceType;
    }

    static bool TryGetProjectionMetadata(FieldType fieldType, [NotNullWhen(true)] out LambdaExpression? projection, [NotNullWhen(true)] out Type? sourceType)
    {
        if (fieldType.Metadata.TryGetValue("_EF_Projection", out var projectionObj) &&
            fieldType.Metadata.TryGetValue("_EF_ProjectionSourceType", out var sourceTypeObj))
        {
            projection = (LambdaExpression)projectionObj!;
            sourceType = (Type)sourceTypeObj!;
            return true;
        }

        projection = null;
        sourceType = null;
        return false;
    }
}
