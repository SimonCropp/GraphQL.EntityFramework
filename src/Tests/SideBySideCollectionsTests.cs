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

    [Test]
    public async Task None() =>
        await Assert.That(SideBySideCollections.Count(Projection(("Parent", false, Empty)))).IsEqualTo(0);

    [Test]
    public async Task One() =>
        await Assert.That(SideBySideCollections.Count(Projection(("Children", true, Empty)))).IsEqualTo(1);

    // A row per grandchild, so the chain is one
    [Test]
    public async Task NestedChainCountsOnce() =>
        await Assert.That(
                SideBySideCollections.Count(
                    Projection(("Children", true, Projection(("Grandchildren", true, Empty))))))
            .IsEqualTo(1);

    [Test]
    public async Task SideBySide() =>
        await Assert.That(
                SideBySideCollections.Count(
                    Projection(
                        ("Children", true, Empty),
                        ("Owner", false, Projection(("Roles", true, Projection(("Departments", true, Empty))))))))
            .IsEqualTo(2);

    // Each child holds its grandchildren and its pets side by side
    [Test]
    public async Task SideBySideInANestedCollection() =>
        await Assert.That(
                SideBySideCollections.Count(
                    Projection(
                        ("Children", true, Projection(
                            ("Grandchildren", true, Empty),
                            ("Pets", true, Empty))))))
            .IsEqualTo(2);

    [Test]
    public async Task DerivedNavigations() =>
        await Assert.That(
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
                    }))
            .IsEqualTo(2);
}
