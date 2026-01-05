class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames)
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

        return GetProjectionInfo(context, navigationProperties, keys ?? []);
    }

    FieldProjectionInfo GetProjectionInfo(
        IResolveFieldContext context,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string> keys)
    {
        var scalarFields = new List<string>();
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

        return new(scalarFields, keys, navProjections);
    }

    void ProcessConnectionNodeFields(
        GraphQLSelectionSet? selectionSet,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string> scalarFields,
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
        List<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Check if this field has include metadata (navigation field with possible alias)
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

                    // Only the first (primary) navigation gets the nested projection from the query
                    // Other navigations get empty projections (select all their keys)
                    var nestedProjection = addedAny
                        ? new([], nestedKeys ?? [], [])
                        : GetNestedProjection(
                            fieldInfo.Field.SelectionSet,
                            nestedNavProps,
                            nestedKeys ?? [],
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

            var nestedProjection = GetNestedProjection(
                fieldInfo.Field.SelectionSet,
                nestedNavProps,
                nestedKeys ?? [],
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

    FieldProjectionInfo GetNestedProjection(
        GraphQLSelectionSet? selectionSet,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string> keys,
        IResolveFieldContext context)
    {
        var scalarFields = new List<string>();
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (selectionSet?.Selections is null)
        {
            return new(scalarFields, keys, navProjections);
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

        return new(scalarFields, keys, navProjections);
    }

    void ProcessNestedProjectionField(
        string fieldName,
        GraphQLField field,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Check if this field is a navigation property
        Navigation? navigation = null;
        navigationProperties?.TryGetValue(fieldName, out navigation);

        if (navigation == null)
        {
            // It's a scalar field - avoid duplicates
            if (!scalarFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                scalarFields.Add(fieldName);
            }
        }
        else
        {
            // It's a navigation - build nested projection
            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);

            var nestedProjection = GetNestedProjection(
                field.SelectionSet,
                nestedNavProps,
                nestedKeys ?? [],
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
                .SingleOrDefault(x => x.FragmentName.Name == name);
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

        if (!TryGetIncludeMetadata(fieldType, out var includeNames))
        {
            return;
        }

        var paths = GetPaths(parentPath, includeNames).ToList();
        list.AddRange(paths);

        ProcessSubFields(list, paths.First(), subFields, graph, navigations[entityType], context);
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
}