// Outside an execution there is no batch to share, so a batch filter runs over the items it is given
public class BatchFilterTests
{
    [Fact]
    public async Task ApplyFilter_passes_the_items_in_one_call()
    {
        var calls = new List<List<string?>>();
        var filters = BuildFilters(calls);
        ParentEntity[] items =
        [
            new()
            {
                Property = "Value1"
            },
            new()
            {
                Property = "Ignore"
            },
            new()
            {
                Property = "Value1"
            },
            new()
            {
                Property = "Value2"
            }
        ];

        var result = await filters.ApplyFilter(items, new(), null!, null);

        Assert.Equal(["Value1", "Value1", "Value2"], result.Select(_ => _.Property));
        var call = Assert.Single(calls);
        Assert.Equal(["Ignore", "Value1", "Value2"], call);
    }

    [Fact]
    public async Task ShouldInclude_passes_the_item()
    {
        var calls = new List<List<string?>>();
        var filters = BuildFilters(calls);

        var kept = await filters.ShouldInclude(
            new(),
            null!,
            null,
            new ParentEntity
            {
                Property = "Value1"
            });
        var ignored = await filters.ShouldInclude(
            new(),
            null!,
            null,
            new ParentEntity
            {
                Property = "Ignore"
            });

        Assert.True(kept);
        Assert.False(ignored);
        Assert.Equal([["Value1"], ["Ignore"]], calls);
    }

    static Filters<IntegrationDbContext> BuildFilters(List<List<string?>> calls)
    {
        var filters = new Filters<IntegrationDbContext>();
        filters.For<ParentEntity>().AddBatch(
            projection: _ => _.Property,
            filter: (_, _, _, properties) =>
            {
                calls.Add(properties.Order().ToList());
                IReadOnlySet<string?> included = properties
                    .Where(_ => _ != "Ignore")
                    .ToHashSet();
                return Task.FromResult(included);
            });
        return filters;
    }
}
