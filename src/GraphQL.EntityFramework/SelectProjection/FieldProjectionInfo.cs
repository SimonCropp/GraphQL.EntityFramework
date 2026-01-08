record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations)
{
    public FieldProjectionInfo MergeFilterFields(IReadOnlySet<string> filterFields)
    {
        var mergedScalars = new List<string>(ScalarFields);

        foreach (var field in filterFields)
        {
            // Only merge simple property names (not navigation paths like "Parent.Name")
            // Navigation paths require the navigation to already be in the GraphQL query
            if (!field.Contains('.') &&
                !mergedScalars.Contains(field, StringComparer.OrdinalIgnoreCase) &&
                !KeyNames.Contains(field, StringComparer.OrdinalIgnoreCase))
            {
                mergedScalars.Add(field);
            }
        }

        return new(mergedScalars, KeyNames, Navigations);
    }

    public FieldProjectionInfo MergeAllFilterFields(IReadOnlyDictionary<Type, IReadOnlySet<string>> allFilterFields, Type entityType)
    {
        // Merge filter fields for this entity type
        var mergedScalars = new List<string>(ScalarFields);
        if (allFilterFields.TryGetValue(entityType, out var filterFields))
        {
            foreach (var field in filterFields)
            {
                // Only merge simple property names (not navigation paths like "Parent.Name")
                if (!field.Contains('.') &&
                    !mergedScalars.Contains(field, StringComparer.OrdinalIgnoreCase) &&
                    !KeyNames.Contains(field, StringComparer.OrdinalIgnoreCase))
                {
                    mergedScalars.Add(field);
                }
            }
        }

        // Recursively merge filter fields for navigation entities
        var mergedNavigations = new Dictionary<string, NavigationProjectionInfo>(Navigations);
        foreach (var (navName, navProjection) in Navigations)
        {
            var mergedNavProjection = navProjection.Projection.MergeAllFilterFields(allFilterFields, navProjection.EntityType);
            mergedNavigations[navName] = navProjection with { Projection = mergedNavProjection };
        }

        return new(mergedScalars, KeyNames, mergedNavigations);
    }
}