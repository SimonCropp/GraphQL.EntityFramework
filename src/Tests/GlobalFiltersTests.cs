using System.Threading.Tasks;
using GraphQL.EntityFramework;
using Xunit;
using Xunit.Abstractions;

public class GlobalFiltersTests :
    XunitApprovalBase
{
    [Fact]
    public async Task Simple()
    {
        var filters= new Filters();
        filters.Add<Target>((o, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new object(), new Target()));
        Assert.False(await filters.ShouldInclude<object>(new object(), null));
        Assert.True(await filters.ShouldInclude(new object(), new Target {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new object(), new Target {Property = "Ignore"}));

        filters.Add<BaseTarget>((o, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new object(), new ChildTarget()));
        Assert.True(await filters.ShouldInclude(new object(), new ChildTarget {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new object(), new ChildTarget {Property = "Ignore"}));

        filters.Add<ITarget>((o, target) => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new object(), new ImplementationTarget()));
        Assert.True(await filters.ShouldInclude(new object(), new ImplementationTarget { Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new object(), new ImplementationTarget { Property = "Ignore" }));

        Assert.True(await filters.ShouldInclude(new object(), new NonTarget { Property = "Foo" }));
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

    public GlobalFiltersTests(ITestOutputHelper output) :
        base(output)
    {
    }
}