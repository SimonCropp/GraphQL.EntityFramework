namespace GraphQL.EntityFramework;

#region FiltersSignature

public class Filters<TDbContext>
    where TDbContext : DbContext
{
    public delegate bool Filter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

    public delegate Task<bool> AsyncFilter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

    public delegate Task<IReadOnlySet<TProjection>> BatchFilter<TProjection>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, IReadOnlyCollection<TProjection> inputs);

    #endregion

    /// <summary>
    /// Create a filter builder for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <returns>A filter builder that supports type inference for projections.</returns>
    /// <example>
    /// <code>
    /// // Single field
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => _.Quantity,
    ///     filter: (_, _, _, qty) => qty > 0);
    ///
    /// // Anonymous type
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => new { _.Quantity, _.Price },
    ///     filter: (_, _, _, x) => x.Quantity > 0 &amp;&amp; x.Price >= 10);
    ///
    /// // Named type
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => new ProductProjection { Quantity = _.Quantity },
    ///     filter: (_, _, _, x) => x.Quantity > 0);
    /// </code>
    /// </example>
    public FilterBuilder<TDbContext, TEntity> For<TEntity>()
        where TEntity : class =>
        new(this);

    internal void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>>? projection,
        AsyncFilter<TProjection> filter)
        where TEntity : class =>
        AddEntry<TEntity>(new FilterEntry<TDbContext, TEntity, TProjection>(
            async (userContext, data, userPrincipal, item) =>
            {
                try
                {
                    return await filter(userContext, data, userPrincipal, item);
                }
                catch (Exception exception)
                {
                    throw new($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                }
            },
            projection));

    internal void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>>? projection,
        Filter<TProjection> filter)
        where TEntity : class =>
        AddEntry<TEntity>(new FilterEntry<TDbContext, TEntity, TProjection>(
            (userContext, data, userPrincipal, item) =>
            {
                try
                {
                    return Task.FromResult(filter(userContext, data, userPrincipal, item));
                }
                catch (Exception exception)
                {
                    throw new($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                }
            },
            projection));

    internal void AddBatch<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        BatchFilter<TProjection> filter)
        where TEntity : class =>
        AddEntry<TEntity>(new BatchFilterEntry<TDbContext, TEntity, TProjection>(filter, projection));

    Dictionary<Type, List<IFilterEntry<TDbContext>>> entries = [];

    /// <summary>
    /// Filters registered for the same entity type are all applied, and a node is included only if
    /// every one of them accepts it. This matches how a filter on a base type already composes with
    /// a filter on a derived type.
    /// </summary>
    void AddEntry<TEntity>(IFilterEntry<TDbContext> entry)
        where TEntity : class
    {
        var type = typeof(TEntity);
        if (!entries.TryGetValue(type, out var forType))
        {
            forType = [];
            entries[type] = forType;
        }

        forType.Add(entry);
        filtersByType.Clear();
        filtersForHierarchy.Clear();
    }

    /// <summary>
    /// Filters are matched against the runtime type of each item rather than the type of the field
    /// that returned it, so a filter on a derived type applies to derived instances in a base typed
    /// list, and a field typed as object is filtered the same as a typed one. Looked up per item,
    /// so the result is cached per type; the cache is reset when a filter is added.
    /// </summary>
    ConcurrentDictionary<Type, TypeFilters> filtersByType = new();

    TypeFilters GetFilters(Type entityType) =>
        filtersByType.GetOrAdd(
            entityType,
            type =>
            {
                var forType = entries
                    .Where(_ => _.Key.IsAssignableFrom(type))
                    .SelectMany(_ => _.Value)
                    .ToList();
                return new(
                    forType.Where(_ => _ is not IBatchFilterEntry<TDbContext>).ToList(),
                    forType.OfType<IBatchFilterEntry<TDbContext>>().ToList());
            });

    // Per item filters run on each item as it is filtered. Batch filters run once over many items.
    sealed record TypeFilters(
        IReadOnlyList<IFilterEntry<TDbContext>> PerItem,
        IReadOnlyList<IBatchFilterEntry<TDbContext>> Batch);

    /// <summary>
    /// The filters whose projection requirements a query for <paramref name="entityType"/> has to
    /// load: those on the type and its base types, which apply to every item, and those on derived
    /// types, which apply to the derived items the query can return.
    /// </summary>
    internal IReadOnlyList<IFilterEntry<TDbContext>> GetFiltersForHierarchy(Type entityType) =>
        filtersForHierarchy.GetOrAdd(
            entityType,
            type => entries
                .Where(_ => _.Key.IsAssignableFrom(type) || type.IsAssignableFrom(_.Key))
                .SelectMany(_ => _.Value)
                .ToList());

    // Looked up for every entity type in a projection on every request, so cached per type the
    // same way as GetFilters, and reset when a filter is added
    ConcurrentDictionary<Type, List<IFilterEntry<TDbContext>>> filtersForHierarchy = new();

    /// <summary>
    /// Returns true if there are any filters registered.
    /// </summary>
    internal bool HasFilters => entries.Count > 0;

    /// <summary>
    /// Without an execution to share, batch filters run once over <paramref name="result"/>.
    /// </summary>
    internal virtual async Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(
        IEnumerable<TEntity> result,
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
        where TEntity : class
    {
        if (entries.Count == 0)
        {
            return result;
        }

        var filtered = await Apply(userContext, userPrincipal, null, data, result, _ => new(_));
        return (IEnumerable<TEntity>)filtered!;
    }

    // One per execution, so the rows an execution resolves share a batch
    ConditionalWeakTable<IExecutionContext, OpenFilterBatch<TDbContext>> openBatches = new();

    /// <summary>
    /// Filters the items a resolver returns and passes the included ones, in order, to
    /// <paramref name="complete"/>, whose result is the field's value. Null items are passed through.
    /// Per item filters run now. Batch filters do not: the items join the execution's open
    /// <see cref="FilterBatch{TDbContext}"/> and a deferred result is returned, so the rows of a
    /// response share one call per batch filter rather than making one each.
    /// </summary>
    internal ValueTask<object?> Apply<TItem>(
        IResolveFieldContext context,
        TDbContext data,
        IEnumerable<TItem> items,
        Func<List<TItem>, ValueTask<object?>> complete) =>
        Apply(context.UserContext, context.User, context.ExecutionContext, data, items, complete);

    async ValueTask<object?> Apply<TItem>(
        object userContext,
        ClaimsPrincipal? userPrincipal,
        IExecutionContext? execution,
        TDbContext data,
        IEnumerable<TItem> items,
        Func<List<TItem>, ValueTask<object?>> complete)
    {
        if (entries.Count == 0)
        {
            return await complete(items as List<TItem> ?? [.. items]);
        }

        var candidates = new List<Candidate<TItem>>();
        List<(IBatchFilterEntry<TDbContext> Entry, object? Projection)>? batchInputs = null;
        foreach (var item in items)
        {
            if (item is null)
            {
                candidates.Add(new(item, null));
                continue;
            }

            var filters = GetFilters(item.GetType());
            if (!await IncludedByPerItemFilters(filters.PerItem, userContext, data, userPrincipal, item))
            {
                continue;
            }

            if (filters.Batch.Count == 0)
            {
                candidates.Add(new(item, null));
                continue;
            }

            var checks = new (IBatchFilterEntry<TDbContext> Entry, object? Projection)[filters.Batch.Count];
            for (var index = 0; index < checks.Length; index++)
            {
                var entry = filters.Batch[index];
                checks[index] = (entry, entry.Project(item));
            }

            candidates.Add(new(item, checks));
            batchInputs ??= [];
            batchInputs.AddRange(checks);
        }

        if (batchInputs is null)
        {
            return await complete(candidates.Select(_ => _.Item).ToList());
        }

        if (execution is null)
        {
            var batch = new FilterBatch<TDbContext>(new());
            batch.Add(batchInputs);
            return await CompleteBatch(batch);
        }

        var shared = openBatches.GetValue(execution, _ => new()).Add(batchInputs);
        return new DeferredFilterResult(async () => await CompleteBatch(shared));

        async ValueTask<object?> CompleteBatch(FilterBatch<TDbContext> batch)
        {
            var results = await batch.Run(userContext, data, userPrincipal);
            var included = new List<TItem>(candidates.Count);
            foreach (var (item, checks) in candidates)
            {
                if (checks is null ||
                    checks.All(_ => results[_.Entry](_.Projection)))
                {
                    included.Add(item);
                }
            }

            return await complete(included);
        }
    }

    // An item that passed the per item filters, and the batch filter checks it still has to pass
    readonly record struct Candidate<TItem>(
        TItem Item,
        (IBatchFilterEntry<TDbContext> Entry, object? Projection)[]? Checks);

    static async Task<bool> IncludedByPerItemFilters(
        IReadOnlyList<IFilterEntry<TDbContext>> filters,
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item)
    {
        foreach (var entry in filters)
        {
            if (!await entry.ShouldIncludeWithProjection(userContext, data, userPrincipal, item))
            {
                return false;
            }
        }

        return true;
    }

    // A single item on its own, so batch filters run over just this item
    async Task<bool> ShouldIncludeItem(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item)
    {
        var filters = GetFilters(item.GetType());
        if (!await IncludedByPerItemFilters(filters.PerItem, userContext, data, userPrincipal, item))
        {
            return false;
        }

        foreach (var entry in filters.Batch)
        {
            if (!await entry.ShouldIncludeWithProjection(userContext, data, userPrincipal, item))
            {
                return false;
            }
        }

        return true;
    }

    internal virtual Task<bool> ShouldInclude<TEntity>(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        TEntity? item)
        where TEntity : class =>
        ShouldInclude(userContext, data, userPrincipal, (object?)item);

    internal Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object? item)
    {
        if (item is null)
        {
            return Task.FromResult(false);
        }

        if (entries.Count == 0)
        {
            return Task.FromResult(true);
        }

        return ShouldIncludeItem(userContext, data, userPrincipal, item);
    }
}
