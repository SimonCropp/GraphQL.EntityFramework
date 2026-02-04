class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames,
    IReadOnlyDictionary<Type, IReadOnlySet<string>> foreignKeys)
{
    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class
    {
        if (HasSelectInQueryChain(query.Expression))
        {
            return query;
        }

        if (context.SubFields is null)
        {
            return query;
        }

        if (!navigations.TryGetValue(typeof(TItem), out var navigationProperty))
        {
            return query;
        }

        var paths = GetIncludePaths(context, navigationProperty);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    /// <summary>
    /// Walks the LINQ method call chain to find Select calls.
    /// When a query has been projected via Select, Include cannot be applied.
    /// </summary>
    static bool HasSelectInQueryChain(Expression expression)
    {
        while (expression is MethodCallExpression methodCall)
        {
            if (methodCall.Method is { Name: "Select", DeclaringType: { } declaringType } &&
                (declaringType == typeof(Queryable) || declaringType == typeof(Enumerable)))
            {
                return true;
            }

            if (methodCall.Arguments.Count > 0)
            {
                expression = methodCall.Arguments[0];
            }
            else if (methodCall.Object != null)
            {
                expression = methodCall.Object;
            }
            else
            {
                break;
            }
        }

        return false;
    }

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
            ProcessProjectionExpression(fieldInfo, projection, navigationProperties, navProjections, context);
            return;
        }

        ProcessNestedProjectionField(fieldName, fieldInfo.Field, navigationProperties, scalarFields, navProjections, context);
    }

    void ProcessProjectionExpression(
        (GraphQLField Field, FieldType FieldType) fieldInfo,
        LambdaExpression projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
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
            if (!TryFindNavigation(navigationProperties, navName, out var navigation, out var actualNavName) ||
                navProjections.ContainsKey(actualNavName))
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
                var scalarFields = nestedPaths
                    .Where(_ => !_.Contains('.'))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                nestedProjection = new(scalarFields, nestedKeys ?? [], nestedFks ?? new HashSet<string>(), []);
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

    List<string> GetIncludePaths(IResolveFieldContext context, IReadOnlyDictionary<string, Navigation> navigationProperty)
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

        if (IsConnectionNodeName(field.Name.StringValue) || field == context.FieldAst)
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

        return includeNames.Select(_ => $"{parentPath}.{_}");
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

    public static void SetIncludeMetadata(FieldType fieldType, string fieldName, IEnumerable<string>? includeNames) =>
        fieldType.Metadata["_EF_IncludeName"] = includeNames?.ToArray() ?? FieldNameToArray(fieldName);

    static string[] FieldNameToArray(string fieldName) =>
        [char.ToUpperInvariant(fieldName[0]) + fieldName[1..]];

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
