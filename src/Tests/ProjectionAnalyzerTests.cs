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

    [Fact]
    public void SimpleProperty()
    {
        Expression<Func<BaseEntity, string>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Name", paths);
    }

    [Fact]
    public void CastToBaseType()
    {
        Expression<Func<DerivedEntity, int>> projection = e => e.Status;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Status", paths);
    }

    [Fact]
    public void CastToInterface()
    {
        Expression<Func<ConcreteEntity, string>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Name", paths);
    }

    [Fact]
    public void CastInNewExpression()
    {
        Expression<Func<DerivedEntity, object>> projection = e => new { e.Status, e.Name };
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Status", paths);
        Assert.Contains("Name", paths);
    }

    [Fact]
    public void AsCast()
    {
        Expression<Func<DerivedEntity, string?>> projection = e => e.Name;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Name", paths);
    }

    [Fact]
    public void NavigationThroughCast()
    {
        Expression<Func<EntityWithNav, string>> projection = e => e.Nav!.Value;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Nav.Value", paths);
    }

    [Fact]
    public void MultipleCasts()
    {
        Expression<Func<DerivedEntity, int>> projection = _ => _.Status;
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Status", paths);
    }

    [Fact]
    public void NoCast()
    {
        Expression<Func<BaseEntity, object>> projection = e => new { e.Name, e.Status };
        var paths = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        Assert.Contains("Name", paths);
        Assert.Contains("Status", paths);
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

    static void AssertPaths(Expression<Func<EntityWithItems, object?>> projection, params string[] expected) =>
        Assert.Equal(
            expected.Order(),
            ProjectionAnalyzer.ExtractPropertyPaths(projection).Order());

    // The properties are read off the lambda's own parameter, so were not recorded, and the
    // collection was loaded whole
    [Fact]
    public void CollectionSelect() =>
        AssertPaths(
            _ => _.Items.Select(item => item.Id).ToList(),
            "Items", "Items.Id");

    [Fact]
    public void CollectionChain() =>
        AssertPaths(
            _ => _.Items
                .Where(item => item.Active)
                .OrderBy(item => item.Name)
                .Select(item => item.Id)
                .ToList(),
            "Items", "Items.Active", "Items.Name", "Items.Id");

    [Fact]
    public void CollectionAggregate() =>
        AssertPaths(
            _ => new
            {
                Any = _.Items.Any(item => item.Active),
                Count = _.Items.Count(item => item.Name == "a")
            },
            "Items", "Items.Active", "Items.Name");

    // The items come out, so whatever uses them can read any property
    [Fact]
    public void CollectionReturningItemsIsWhole() =>
        AssertPaths(
            _ => _.Items.Where(item => item.Active).ToList(),
            "Items");

    [Fact]
    public void CollectionReturningAnItemIsWhole() =>
        AssertPaths(
            _ => _.Items.FirstOrDefault(item => item.Active),
            "Items");

    [Fact]
    public void CollectionSelectingTheItemIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(item => new { item, item.Name }).ToList(),
            "Items");

    // A navigation of an item is not a column of it
    [Fact]
    public void CollectionReadingItemNavigationIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(item => item.Nav!.Value).ToList(),
            "Items");

    [Fact]
    public void CollectionPassingItemToMethodIsWhole() =>
        AssertPaths(
            _ => _.Items.Select(Describe).ToList(),
            "Items");

    // Nothing says which properties a count needs, so this is unchanged
    [Fact]
    public void CollectionWithNoLambdaIsWhole() =>
        AssertPaths(
            _ => _.Items.Count(),
            "Items");

    static string Describe(Item item) =>
        item.Name;
}
