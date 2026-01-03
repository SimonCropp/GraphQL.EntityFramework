class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> navigations,
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

        return GetProjectionInfo(context, type, navigationProperties, keys ?? []);
    }

    FieldProjectionInfo GetProjectionInfo(
        IResolveFieldContext context,
        Type entityType,
        IReadOnlyList<Navigation>? navigationProperties,
        List<string> keys)
    {
        var scalarFields = new List<string>();
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (context.SubFields is not null)
        {
            foreach (var (fieldName, fieldAst) in context.SubFields)
            {
                ProcessProjectionField(fieldName, fieldAst, entityType, navigationProperties, scalarFields, navProjections, context);
            }
        }

        return new FieldProjectionInfo(scalarFields, keys, navProjections);
    }

    void ProcessProjectionField(
        string fieldName,
        (GraphQLField Field, FieldType FieldType) fieldInfo,
        Type entityType,
        IReadOnlyList<Navigation>? navigationProperties,
        List<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Check if this field has include metadata (navigation field with possible alias)
        if (TryGetIncludeMetadata(fieldInfo.FieldType, out var includeNames))
        {
            // It's a navigation field - use the include name to find the navigation
            var navName = includeNames[0]; // Primary navigation name
            var navigation = navigationProperties?.FirstOrDefault(n =>
                n.Name.Equals(navName, StringComparison.OrdinalIgnoreCase));

            if (navigation != null)
            {
                var navType = navigation.Type;
                navigations.TryGetValue(navType, out var nestedNavProps);
                keyNames.TryGetValue(navType, out var nestedKeys);

                var nestedProjection = GetNestedProjection(
                    fieldInfo.Field.SelectionSet,
                    navType,
                    nestedNavProps,
                    nestedKeys ?? [],
                    context);

                navProjections[navigation.Name] = new NavigationProjectionInfo(
                    navType,
                    navigation.IsCollection,
                    nestedProjection);
                return;
            }
        }

        // Check if this field is a navigation property by name
        var navByName = navigationProperties?.FirstOrDefault(n =>
            n.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

        if (navByName != null)
        {
            // It's a navigation - build nested projection
            var navType = navByName.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);

            var nestedProjection = GetNestedProjection(
                fieldInfo.Field.SelectionSet,
                navType,
                nestedNavProps,
                nestedKeys ?? [],
                context);

            navProjections[navByName.Name] = new NavigationProjectionInfo(
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
        Type entityType,
        IReadOnlyList<Navigation>? navigationProperties,
        List<string> keys,
        IResolveFieldContext context)
    {
        var scalarFields = new List<string>();
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (selectionSet?.Selections is null)
        {
            return new FieldProjectionInfo(scalarFields, keys, navProjections);
        }

        // Process direct fields
        foreach (var selection in selectionSet.Selections.OfType<GraphQLField>())
        {
            var fieldName = selection.Name.StringValue;
            ProcessNestedProjectionField(fieldName, selection, entityType, navigationProperties, scalarFields, navProjections, context);
        }

        // Process inline fragments
        foreach (var inlineFragment in selectionSet.Selections.OfType<GraphQLInlineFragment>())
        {
            if (inlineFragment.SelectionSet?.Selections is not null)
            {
                foreach (var selection in inlineFragment.SelectionSet.Selections.OfType<GraphQLField>())
                {
                    var fieldName = selection.Name.StringValue;
                    ProcessNestedProjectionField(fieldName, selection, entityType, navigationProperties, scalarFields, navProjections, context);
                }
            }
        }

        // Process fragment spreads
        foreach (var fragmentSpread in selectionSet.Selections.OfType<GraphQLFragmentSpread>())
        {
            var name = fragmentSpread.FragmentName.Name;
            var fragmentDefinition = context.Document.Definitions
                .OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(x => x.FragmentName.Name == name);

            if (fragmentDefinition?.SelectionSet?.Selections is not null)
            {
                foreach (var selection in fragmentDefinition.SelectionSet.Selections.OfType<GraphQLField>())
                {
                    var fieldName = selection.Name.StringValue;
                    ProcessNestedProjectionField(fieldName, selection, entityType, navigationProperties, scalarFields, navProjections, context);
                }
            }
        }

        return new FieldProjectionInfo(scalarFields, keys, navProjections);
    }

    void ProcessNestedProjectionField(
        string fieldName,
        GraphQLField field,
        Type entityType,
        IReadOnlyList<Navigation>? navigationProperties,
        List<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // Check if this field is a navigation property
        var navigation = navigationProperties?.FirstOrDefault(n =>
            n.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

        if (navigation != null)
        {
            // It's a navigation - build nested projection
            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);

            var nestedProjection = GetNestedProjection(
                field.SelectionSet,
                navType,
                nestedNavProps,
                nestedKeys ?? [],
                context);

            navProjections[navigation.Name] = new NavigationProjectionInfo(
                navType,
                navigation.IsCollection,
                nestedProjection);
        }
        else
        {
            // It's a scalar field - avoid duplicates
            if (!scalarFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                scalarFields.Add(fieldName);
            }
        }
    }

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, IResolveFieldContext context, IReadOnlyList<Navigation> navigationProperties)
        where T : class
    {
        var paths = GetPaths(context, navigationProperties);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    List<string> GetPaths(IResolveFieldContext context, IReadOnlyList<Navigation> navigationProperty)
    {
        var list = new List<string>();

        AddField(list, context.FieldAst, context.FieldAst.SelectionSet!, null, context.FieldDefinition, navigationProperty, context);

        return list;
    }

    void AddField(List<string> list, GraphQLField field, GraphQLSelectionSet selectionSet, string? parentPath, FieldType fieldType, IReadOnlyList<Navigation> parentNavigationProperties, IResolveFieldContext context, IComplexGraphType? graph = null)
    {
        if (graph == null && !fieldType.TryGetComplexGraph(out graph))
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
                .SingleOrDefault(x=>x.FragmentName.Name == name);
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
        foreach (var path in paths)
        {
            list.Add(path);
        }

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

    void ProcessSubFields(List<string> list, string? parentPath, List<GraphQLField> subFields, IComplexGraphType graph, IReadOnlyList<Navigation> navigationProperties, IResolveFieldContext context)
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
        [ char.ToUpperInvariant(fieldName[0]) + fieldName[1..] ];

    static bool TryGetIncludeMetadata(FieldType fieldType, [NotNullWhen(true)] out string[]? value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[])fieldNameObject!;
            return true;
        }

        value = null;
        return false;
    }
}