/// <summary>
/// Counts the collections a projection loads side by side. A single query joins every collection
/// it loads, and collections side by side multiply each other's rows. A collection nested in
/// another does not, since the query returns a row per item of the inner one, so a chain of nested
/// collections counts as one. A collection holding several side by side counts as many as it
/// holds. A reference navigation is not a collection, but passes on the collections under it.
/// </summary>
static class SideBySideCollections
{
    public static int Count(FieldProjectionInfo projection)
    {
        var count = 0;
        if (projection.Navigations is not null)
        {
            count += Count(projection.Navigations);
        }

        if (projection.DerivedNavigations is not null)
        {
            foreach (var navigations in projection.DerivedNavigations.Values)
            {
                count += Count(navigations);
            }
        }

        return count;
    }

    static int Count(Dictionary<string, NavigationProjectionInfo> navigations)
    {
        var count = 0;
        foreach (var navigation in navigations.Values)
        {
            var inner = Count(navigation.Projection);
            if (navigation.IsCollection)
            {
                count += Math.Max(1, inner);
            }
            else
            {
                count += inner;
            }
        }

        return count;
    }
}
