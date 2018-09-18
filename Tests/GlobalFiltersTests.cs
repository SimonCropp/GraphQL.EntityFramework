using GraphQL.EntityFramework;
using Xunit;

public class GlobalFiltersTests
{
    [Fact]
    public void Simple()
    {
        GlobalFilters.Add<Target>((o, target) => target.Property != "Ignore");
        Assert.True(GlobalFilters.ShouldInclude(null, new Target()));
        Assert.False(GlobalFilters.ShouldInclude(null, null));
        Assert.True(GlobalFilters.ShouldInclude(null, new Target {Property = "Include"}));
        Assert.False(GlobalFilters.ShouldInclude(null, new Target {Property = "Ignore"}));

        GlobalFilters.Add<BaseTarget>((o, target) => target.Property != "Ignore");
        Assert.True(GlobalFilters.ShouldInclude(null, new ChildTarget()));
        Assert.True(GlobalFilters.ShouldInclude(null, new ChildTarget {Property = "Include"}));
        Assert.False(GlobalFilters.ShouldInclude(null, new ChildTarget {Property = "Ignore"}));

        GlobalFilters.Add<ITarget>((o, target) => target.Property != "Ignore");
        Assert.True(GlobalFilters.ShouldInclude(null, new ImplementationTarget()));
        Assert.True(GlobalFilters.ShouldInclude(null, new ImplementationTarget { Property = "Include"}));
        Assert.False(GlobalFilters.ShouldInclude(null, new ImplementationTarget { Property = "Ignore" }));

        Assert.True(GlobalFilters.ShouldInclude(null, new NonTarget { Property = "Foo" }));
    }

    public class NonTarget
    {
        public string Property { get; set; }
    }
    public class Target
    {
        public string Property { get; set; }
    }

    public class ChildTarget : BaseTarget
    {
    }

    public class BaseTarget
    {
        public string Property { get; set; }
    }

    public class ImplementationTarget : ITarget
    {
        public string Property { get; set; }
    }

    public interface ITarget
    {
        string Property { get; set; }
    }
}