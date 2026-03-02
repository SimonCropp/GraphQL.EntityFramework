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
}
