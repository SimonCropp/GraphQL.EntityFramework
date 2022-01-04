using Filters = GraphQL.EntityFramework.Filters;

public class GlobalFiltersTests
{
    [Fact]
    public async Task Simple()
    {
        var filters= new Filters();
        filters.Add<Target>((_, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new Target()));
        Assert.False(await filters.ShouldInclude<object>(new(), null));
        Assert.True(await filters.ShouldInclude(new(), new Target {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new Target {Property = "Ignore"}));

        filters.Add<BaseTarget>((_, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new ChildTarget()));
        Assert.True(await filters.ShouldInclude(new(), new ChildTarget {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new ChildTarget {Property = "Ignore"}));

        filters.Add<ITarget>((_, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new ImplementationTarget()));
        Assert.True(await filters.ShouldInclude(new(), new ImplementationTarget { Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new ImplementationTarget { Property = "Ignore" }));

        Assert.True(await filters.ShouldInclude(new(), new NonTarget { Property = "Foo" }));
    }

    public class NonTarget
    {
        public string? Property { get; set; }
    }
    public class Target
    {
        public string? Property { get; set; }
    }

    public class ChildTarget :
        BaseTarget
    {
    }

    public class BaseTarget
    {
        public string? Property { get; set; }
    }

    public class ImplementationTarget :
        ITarget
    {
        public string? Property { get; set; }
    }

    public interface ITarget
    {
        string? Property { get; set; }
    }
}