record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    IReadOnlySet<string> ForeignKeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations)
{
    public FieldProjectionInfo MergeAllFilterFields(
        IReadOnlyDictionary<Type, IReadOnlySet<string>> allFilterFields,
        Type entityType,
        IReadOnlyDictionary<string, Navigation>? navigationMetadata = null)
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
                    // Also exclude navigation property names to prevent loading entire navigations
                    if (!field.Contains('.') &&
                        !mergedScalars.Contains(field, StringComparer.OrdinalIgnoreCase) &&
                        !KeyNames.Contains(field, StringComparer.OrdinalIgnoreCase) &&
                        !(navigationMetadata?.ContainsKey(field) == true)) // NEW: Skip navigation properties
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
            // For recursive calls, we don't have navigation metadata, so pass null
            var mergedNavProjection = navProjection.Projection.MergeAllFilterFields(allFilterFields, navProjection.EntityType, null);
            mergedNavigations[navName] = navProjection with { Projection = mergedNavProjection };
        }

        return new(mergedScalars, KeyNames, ForeignKeyNames, mergedNavigations);
    }
}