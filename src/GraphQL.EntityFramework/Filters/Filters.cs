namespace GraphQL.EntityFramework;

#region FiltersSignature

public class Filters<TDbContext>
    where TDbContext : DbContext
{
    public delegate bool Filter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

    public delegate Task<bool> AsyncFilter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

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
    }

    /// <summary>
    /// Filters are matched against the runtime type of each item rather than the type of the field
    /// that returned it, so a filter on a derived type applies to derived instances in a base typed
    /// list, and a field typed as object is filtered the same as a typed one. Looked up per item,
    /// so the result is cached per type; the cache is reset when a filter is added.
    /// </summary>
    ConcurrentDictionary<Type, List<IFilterEntry<TDbContext>>> filtersByType = new();

    List<IFilterEntry<TDbContext>> GetFilters(Type entityType) =>
        filtersByType.GetOrAdd(
            entityType,
            type => entries
                .Where(_ => _.Key.IsAssignableFrom(type))
                .SelectMany(_ => _.Value)
                .ToList());

    /// <summary>
    /// The filters whose projection requirements a query for <paramref name="entityType"/> has to
    /// load: those on the type and its base types, which apply to every item, and those on derived
    /// types, which apply to the derived items the query can return.
    /// </summary>
    internal IEnumerable<IFilterEntry<TDbContext>> GetFiltersForHierarchy(Type entityType) =>
        entries
            .Where(_ => _.Key.IsAssignableFrom(entityType) || entityType.IsAssignableFrom(_.Key))
            .SelectMany(_ => _.Value);

    /// <summary>
    /// Returns true if there are any filters registered.
    /// </summary>
    internal bool HasFilters => entries.Count > 0;

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

        var list = new List<TEntity>();
        foreach (var item in result)
        {
            if (await ShouldIncludeItem(userContext, data, userPrincipal, item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    async Task<bool> ShouldIncludeItem(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item)
    {
        foreach (var entry in GetFilters(item.GetType()))
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
