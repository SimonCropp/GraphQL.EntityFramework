public class SideBySideCollectionsTests
{
    static FieldProjectionInfo Projection(params (string Name, bool IsCollection, FieldProjectionInfo Projection)[] navigations) =>
        new(
            [],
            null,
            null,
            navigations.ToDictionary(
                _ => _.Name,
                _ => new NavigationProjectionInfo(typeof(object), _.IsCollection, _.Projection)));

    static FieldProjectionInfo Empty => Projection();

    [Fact]
    public void None() =>
        Assert.Equal(0, SideBySideCollections.Count(Projection(("Parent", false, Empty))));

    [Fact]
    public void One() =>
        Assert.Equal(1, SideBySideCollections.Count(Projection(("Children", true, Empty))));

    // A row per grandchild, so the chain is one
    [Fact]
    public void NestedChainCountsOnce() =>
        Assert.Equal(
            1,
            SideBySideCollections.Count(
                Projection(("Children", true, Projection(("Grandchildren", true, Empty))))));

    [Fact]
    public void SideBySide() =>
        Assert.Equal(
            2,
            SideBySideCollections.Count(
                Projection(
                    ("Children", true, Empty),
                    ("Owner", false, Projection(("Roles", true, Projection(("Departments", true, Empty))))))));

    // Each child holds its grandchildren and its pets side by side
    [Fact]
    public void SideBySideInANestedCollection() =>
        Assert.Equal(
            2,
            SideBySideCollections.Count(
                Projection(
                    ("Children", true, Projection(
                        ("Grandchildren", true, Empty),
                        ("Pets", true, Empty))))));

    [Fact]
    public void DerivedNavigations() =>
        Assert.Equal(
            2,
            SideBySideCollections.Count(
                Projection(("Children", true, Empty)) with
                {
                    DerivedNavigations = new()
                    {
                        [typeof(string)] = new()
                        {
                            ["Items"] = new(typeof(object), true, Empty)
                        }
                    }
                }));
}
