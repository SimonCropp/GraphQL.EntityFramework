record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    IReadOnlySet<string> ForeignKeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations)
{
    public FieldProjectionInfo MergeAllFilterFields(IReadOnlyDictionary<Type, IReadOnlySet<string>> allFilterFields, Type entityType)
    {
        // Merge filter fields for this entity type
        var mergedScalars = new List<string>(ScalarFields);

        // Check filter fields for the entity type and all its base types
        // This handles TPH inheritance where filters are defined on base types
        foreach (var (filterType, filterFields) in allFilterFields)
        {
            // Include filter fields if:
            // 1. Exact type match (filterType == entityType), OR
            // 2. Filter is for a base type (filterType.IsAssignableFrom(entityType))
            if (filterType.IsAssignableFrom(entityType))
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
        }

        // Recursively merge filter fields for navigation entities
        var mergedNavigations = new Dictionary<string, NavigationProjectionInfo>(Navigations);
        foreach (var (navName, navProjection) in Navigations)
        {
            var mergedNavProjection = navProjection.Projection.MergeAllFilterFields(allFilterFields, navProjection.EntityType);
            mergedNavigations[navName] = navProjection with { Projection = mergedNavProjection };
        }

        return new(mergedScalars, KeyNames, ForeignKeyNames, mergedNavigations);
    }
}