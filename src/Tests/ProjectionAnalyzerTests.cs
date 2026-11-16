// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
public class ProjectionAnalyzerTests
{
    class BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Status { get; set; }
    }

    class DerivedEntity : BaseEntity
    {
        public string Extra { get; set; } = null!;
    }

    interface IEntity
    {
        Guid Id { get; }
        string Name { get; }
    }

    class ConcreteEntity : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Score { get; set; }
    }

    class Nav
    {
        public string Value { get; set; } = null!;
    }

    class EntityWithNav : BaseEntity
    {
        public Nav? Nav { get; set; }
    }

    [Test]
    public async Task SimpleProperty()
    {
        Expression<Func<BaseEntity, string>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Name");
    }

    [Test]
    public async Task CastToBaseType()
    {
        Expression<Func<DerivedEntity, int>> projection = e => e.Status;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Status");
    }

    [Test]
    public async Task CastToInterface()
    {
        Expression<Func<ConcreteEntity, string>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Name");
    }

    [Test]
    public async Task CastInNewExpression()
    {
        Expression<Func<DerivedEntity, object>> projection = e => new { e.Status, e.Name };
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Status");
        await Assert.That(paths).Contains("Name");
    }

    [Test]
    public async Task AsCast()
    {
        Expression<Func<DerivedEntity, string?>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Name");
    }

    [Test]
    public async Task NavigationThroughCast()
    {
        Expression<Func<EntityWithNav, string>> projection = e => e.Nav!.Value;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Nav.Value");
    }

    [Test]
    public async Task MultipleCasts()
    {
        Expression<Func<DerivedEntity, int>> projection = _ => _.Status;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Status");
    }

    [Test]
    public async Task NoCast()
    {
        Expression<Func<BaseEntity, object>> projection = e => new { e.Name, e.Status };
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        await Assert.That(paths).Contains("Name");
        await Assert.That(paths).Contains("Status");
    }

    class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool Active { get; set; }
        public Nav? Nav { get; set; }
    }

    class EntityWithItems : BaseEntity
    {
        public List<Item> Items { get; set; } = null!;
    }

    static async Task AssertPaths(Expression<Func<EntityWithItems, object?>> projection, params string[] expected) =>
        await Assert.That(ProjectionAnalyzer.ExtractPropertyPaths(projection).Order())
            .IsEquivalentTo(expected.Order(), CollectionOrdering.Matching);

    // The properties are read off the lambda's own parameter, so were not recorded, and the
    // collection was loaded whole
    [Test]
    public Task CollectionSelect() =>
        AssertPaths(
            _ => _.Items.Select(item => item.Id).ToList(),
            "Items", "Items.Id");

    [Test]
    public Task CollectionChain() =>
        AssertPaths(
            _ => _.Items
                .Where(item => item.Active)
                .OrderBy(item => item.Name)
                .Select(item => item.Id)
                .ToList(),
            "Items", "Items.Active", "Items.Name", "Items.Id");

    [Test]
    public Task CollectionAggregate() =>
        AssertPaths(
            _ => new
            {
                Any = _.Items.Any(item => item.Active),
                Count = _.Items.Count(item => item.Name == "a")
            },
            "Items", "Items.Active", "Items.Name");

    // The items come out, so whatever uses them can read any property
    [Test]
    public Task CollectionReturningItemsIsWhole() =>
        AssertPaths(
            _ => _.Items.Where(item => item.Active).ToList(),
            "Items");

    [Test]
    public Task CollectionReturningAnItemIsWhole() =>
        AssertPaths(
            _ => _.Items.FirstOrDefault(item => item.Active),
            "Items");

    [Test]
    public Task CollectionSelectingTheItemIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(item => new { item, item.Name }).ToList(),
            "Items");

    // A navigation of an item is not a column of it
    [Test]
    public Task CollectionReadingItemNavigationIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(item => item.Nav!.Value).ToList(),
            "Items");

    [Test]
    public Task CollectionPassingItemToMethodIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(Describe).ToList(),
            "Items");

    // Nothing says which properties a count needs, so this is unchanged
    [Test]
    public Task CollectionWithNoLambdaIsWhole() =>
        AssertPaths(
            _ => _.Items.Count(),
            "Items");

    static string Describe(Item item) =>
        item.Name;
}
