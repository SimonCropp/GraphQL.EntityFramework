using GraphQL.DataLoader;

/// <summary>
/// Items waiting on batch filters. Resolvers add their items and return a <see cref="DeferredFilterResult"/>.
/// GraphQL.NET completes deferred results only once every other pending field has resolved, so the
/// first of them to complete runs each batch filter once, over the items of every row, and the rest
/// read that outcome.
/// </summary>
class FilterBatch<TDbContext>(object gate)
    where TDbContext : DbContext
{
    Dictionary<IBatchFilterEntry<TDbContext>, HashSet<object?>> inputs = [];
    Task<Dictionary<IBatchFilterEntry<TDbContext>, Func<object?, bool>>>? run;

    // Called under the gate. Once running, the batch takes no more items.
    public bool Started => run is not null;

    // Called under the gate
    public void Add(IReadOnlyList<(IBatchFilterEntry<TDbContext> Entry, object? Projection)> items)
    {
        foreach (var (entry, projection) in items)
        {
            if (!inputs.TryGetValue(entry, out var projections))
            {
                projections = [];
                inputs[entry] = projections;
            }

            projections.Add(projection);
        }
    }

    public Task<Dictionary<IBatchFilterEntry<TDbContext>, Func<object?, bool>>> Run(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
    {
        lock (gate)
        {
            return run ??= RunEntries(userContext, data, userPrincipal);
        }
    }

    async Task<Dictionary<IBatchFilterEntry<TDbContext>, Func<object?, bool>>> RunEntries(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
    {
        var results = new Dictionary<IBatchFilterEntry<TDbContext>, Func<object?, bool>>(inputs.Count);
        foreach (var (entry, projections) in inputs)
        {
            results[entry] = await entry.Filter(userContext, data, userPrincipal, projections);
        }

        return results;
    }
}

/// <summary>
/// The batch that items join during one execution. Once that batch starts running, items start a new
/// one. A resolver's items are added together, so they always land in the same batch.
/// </summary>
class OpenFilterBatch<TDbContext>
    where TDbContext : DbContext
{
    FilterBatch<TDbContext>? batch;

    public FilterBatch<TDbContext> Add(IReadOnlyList<(IBatchFilterEntry<TDbContext> Entry, object? Projection)> items)
    {
        lock (this)
        {
            if (batch is null || batch.Started)
            {
                batch = new(this);
            }

            batch.Add(items);
            return batch;
        }
    }
}

sealed class DeferredFilterResult(Func<Task<object?>> resolve) :
    IDataLoaderResult
{
    public Task<object?> GetResultAsync(Cancel cancel = default) =>
        resolve();
}
