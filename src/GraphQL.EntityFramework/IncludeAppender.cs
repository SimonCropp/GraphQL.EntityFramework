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
