record FieldProjectionInfo(
    HashSet<string> ScalarFields,
    List<string>? KeyNames,
    IReadOnlySet<string>? ForeignKeyNames,
    Dictionary<string, NavigationProjectionInfo>? Navigations,
    Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? DerivedNavigations = null)
{
    /// <summary>
    /// Combine two projections of the same entity type, so that a field requested by either is
    /// projected. Navigations present in both are merged recursively.
    /// </summary>
    public FieldProjectionInfo Merge(FieldProjectionInfo other)
    {
        var scalarFields = new HashSet<string>(ScalarFields, StringComparer.OrdinalIgnoreCase);
        scalarFields.UnionWith(other.ScalarFields);

        return new(
            scalarFields,
            KeyNames ?? other.KeyNames,
            ForeignKeyNames ?? other.ForeignKeyNames,
            MergeNavigations(Navigations, other.Navigations),
            MergeDerivedNavigations(DerivedNavigations, other.DerivedNavigations));
    }

    static Dictionary<string, NavigationProjectionInfo>? MergeNavigations(
        Dictionary<string, NavigationProjectionInfo>? left,
        Dictionary<string, NavigationProjectionInfo>? right)
    {
        if (left is null)
        {
            return right;
        }

        if (right is null)
        {
            return left;
        }

        var merged = new Dictionary<string, NavigationProjectionInfo>(left);
        foreach (var (name, navigation) in right)
        {
            merged[name] = merged.TryGetValue(name, out var existing)
                ? existing.Merge(navigation)
                : navigation;
        }

        return merged;
    }

    static Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? MergeDerivedNavigations(
        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? left,
        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? right)
    {
        if (left is null)
        {
            return right;
        }

        if (right is null)
        {
            return left;
        }

        var merged = new Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>(left);
        foreach (var (type, navigations) in right)
        {
            merged[type] = merged.TryGetValue(type, out var existing)
                ? MergeNavigations(existing, navigations)!
                : navigations;
        }

        return merged;
    }
}
