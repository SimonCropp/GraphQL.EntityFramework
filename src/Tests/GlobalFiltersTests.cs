public class GlobalFiltersTests
{
    [Fact]
    public async Task Simple()
    {
        var filters = new Filters<MyContext>();
        filters.Add<Target>((_, _, _, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new(), null, new Target()));
        Assert.False(await filters.ShouldInclude<object>(new(), new(), null, null));
        Assert.True(await filters.ShouldInclude(new(), new(), null, new Target {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new(), null, new Target {Property = "Ignore"}));

        filters.Add<BaseTarget>((_, _, _,  target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new(), null, new ChildTarget()));
        Assert.True(await filters.ShouldInclude(new(), new(), null, new ChildTarget {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new(), null, new ChildTarget {Property = "Ignore"}));

        filters.Add<ITarget>((_, _, _,  target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new(), new(), null, new ImplementationTarget()));
        Assert.True(await filters.ShouldInclude(new(), new(), null, new ImplementationTarget { Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new(), new(), null, new ImplementationTarget { Property = "Ignore" }));

        Assert.True(await filters.ShouldInclude(new(), new(), null, new NonTarget { Property = "Foo" }));
    }

    public class MyContext : DbContext;

    public class NonTarget
    {
        public string? Property { get; set; }
    }
    public class Target
    {
        public string? Property { get; set; }
    }

    public class ChildTarget :
        BaseTarget;

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