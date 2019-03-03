using GraphQL.EntityFramework;
using Xunit;

public class GlobalFiltersTests
{
    [Fact]
    public void Simple()
    {
        var filters= new GlobalFilters();
        filters.Add<Target>((o, target) => target.Property != "Ignore");
        Assert.True(filters.ShouldInclude(null, new Target()));
        Assert.False(filters.ShouldInclude<object>(null, null));
        Assert.True(filters.ShouldInclude(null, new Target {Property = "Include"}));
        Assert.False(filters.ShouldInclude(null, new Target {Property = "Ignore"}));

        filters.Add<BaseTarget>((o, target) => target.Property != "Ignore");
        Assert.True(filters.ShouldInclude(null, new ChildTarget()));
        Assert.True(filters.ShouldInclude(null, new ChildTarget {Property = "Include"}));
        Assert.False(filters.ShouldInclude(null, new ChildTarget {Property = "Ignore"}));

        filters.Add<ITarget>((o, target) => target.Property != "Ignore");
        Assert.True(filters.ShouldInclude(null, new ImplementationTarget()));
        Assert.True(filters.ShouldInclude(null, new ImplementationTarget { Property = "Include"}));
        Assert.False(filters.ShouldInclude(null, new ImplementationTarget { Property = "Ignore" }));

        Assert.True(filters.ShouldInclude(null, new NonTarget { Property = "Foo" }));
    }

    public class NonTarget
    {
        public string Property { get; set; }
    }
    public class Target
    {
        public string Property { get; set; }
    }

    public class ChildTarget :
        BaseTarget
    {
    }

    public class BaseTarget
    {
        public string Property { get; set; }
    }

    public class ImplementationTarget :
        ITarget
    {
        public string Property { get; set; }
    }

    public interface ITarget
    {
        string Property { get; set; }
    }
}