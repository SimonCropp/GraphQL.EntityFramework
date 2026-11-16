// Outside an execution there is no batch to share, so a batch filter runs over the items it is given
public class BatchFilterTests
{
    [Test]
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

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) ["Value1", "Value1", "Value2"], CollectionOrdering.Matching);
        var call = await Assert.That(calls).HasSingleItem();
        await Assert.That(call).IsEquivalentTo((string?[]) ["Ignore", "Value1", "Value2"], CollectionOrdering.Matching);
    }

    [Test]
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

        await Assert.That(kept).IsTrue();
        await Assert.That(ignored).IsFalse();
        await Assert.That(calls).Count().IsEqualTo(2);
        await Assert.That(calls[0]).IsEquivalentTo((string?[]) ["Value1"], CollectionOrdering.Matching);
        await Assert.That(calls[1]).IsEquivalentTo((string?[]) ["Ignore"], CollectionOrdering.Matching);
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
