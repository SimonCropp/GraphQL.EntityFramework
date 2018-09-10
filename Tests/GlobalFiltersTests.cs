using System.Threading.Tasks;
using GraphQL.EntityFramework;
using Xunit;

public class GlobalFiltersTests
{
    [Fact]
    public async Task Simple()
    {
        GlobalFilters.Add((context, token) => { return Task.FromResult(new Filter<Target>(input => input.Property != "Ignore")); });
        var targetFilter = await GlobalFilters.GetFilter<Target>(null);
        Assert.True(targetFilter(new Target()));
        Assert.True(targetFilter(new Target {Property = "Include"}));
        Assert.False(targetFilter(new Target {Property = "Ignore"}));

        GlobalFilters.Add((context, token) => { return Task.FromResult(new Filter<BaseTarget>(input => input.Property != "Ignore")); });
        var childFilter = await GlobalFilters.GetFilter<ChildTarget>(null);
        Assert.True(childFilter(new ChildTarget()));
        Assert.True(childFilter(new ChildTarget {Property = "Include"}));
        Assert.False(childFilter(new ChildTarget {Property = "Ignore"}));

        GlobalFilters.Add((context, token) => { return Task.FromResult(new Filter<ITarget>(input => input.Property != "Ignore")); });
        var implementationFilter = await GlobalFilters.GetFilter<ImplementationTarget>(null);
        Assert.True(implementationFilter(new ImplementationTarget()));
        Assert.True(implementationFilter(new ImplementationTarget {Property = "Include"}));
        Assert.False(implementationFilter(new ImplementationTarget {Property = "Ignore"}));

        Assert.Null(await GlobalFilters.GetFilter<NonTarget>(null));
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