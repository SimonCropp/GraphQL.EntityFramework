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
        entries[typeof(TEntity)] = new FilterEntry<TDbContext, TEntity, TProjection>(
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
            projection);

    internal void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>>? projection,
        Filter<TProjection> filter)
        where TEntity : class =>
        entries[typeof(TEntity)] = new FilterEntry<TDbContext, TEntity, TProjection>(
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
            projection);

    Dictionary<Type, IFilterEntry<TDbContext>> entries = [];

    /// <summary>
    /// Get all filters that apply to the specified entity type (including base type filters).
    /// </summary>
    internal IEnumerable<IFilterEntry<TDbContext>> GetFilters<TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        return entries
            .Where(_ => _.Key.IsAssignableFrom(type))
            .Select(_ => _.Value);
    }

    /// <summary>
    /// Get all filters that apply to the specified entity type (including base type filters).
    /// </summary>
    internal IEnumerable<IFilterEntry<TDbContext>> GetFilters(Type entityType) =>
        entries
            .Where(_ => _.Key.IsAssignableFrom(entityType))
            .Select(_ => _.Value);

    /// <summary>
    /// Get all registered filter entries.
    /// </summary>
    internal IEnumerable<IFilterEntry<TDbContext>> GetAllFilters() =>
        entries.Values;

    /// <summary>
    /// Returns true if there are any filters registered.
    /// </summary>
    internal bool HasFilters => entries.Count > 0;

    /// <summary>
    /// Get the required property names for filters that apply to the specified entity type.
    /// This aggregates raw property requirements from all applicable filters.
    /// </summary>
    public IReadOnlySet<string> GetRequiredFilterProperties<TEntity>()
        where TEntity : class
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entityType = typeof(TEntity);

        foreach (var (filterType, entry) in entries)
        {
            if (filterType.IsAssignableFrom(entityType))
            {
                foreach (var prop in entry.RequiredPropertyNames)
                {
                    result.Add(prop);
                }
            }
        }

        return result;
    }

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

        var filterEntries = GetFilters<TEntity>().ToList();
        if (filterEntries.Count == 0)
        {
            return result;
        }

        var list = new List<TEntity>();
        foreach (var item in result)
        {
            if (await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries))
            {
                list.Add(item);
            }
        }

        return list;
    }

    static async Task<bool> ShouldIncludeItem<TEntity>(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        TEntity item,
        List<IFilterEntry<TDbContext>> filterEntries)
        where TEntity : class
    {
        foreach (var entry in filterEntries)
        {
            if (!await entry.ShouldIncludeWithProjection(userContext, data, userPrincipal, item))
            {
                return false;
            }
        }

        return true;
    }

    internal virtual async Task<bool> ShouldInclude<TEntity>(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        TEntity? item)
        where TEntity : class
    {
        if (item is null)
        {
            return false;
        }

        if (entries.Count == 0)
        {
            return true;
        }

        var filterEntries = GetFilters<TEntity>().ToList();
        if (filterEntries.Count == 0)
        {
            return true;
        }

        return await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries);
    }
}
