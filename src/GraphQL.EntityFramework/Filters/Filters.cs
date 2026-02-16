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

    /// <summary>
    /// Check if a single entity should be included based on registered filters.
    /// Useful for testing entity filters directly without requiring a full GraphQL query.
    /// Filters are matched using the runtime type of <paramref name="item"/> via <see cref="Type.IsAssignableFrom"/>,
    /// so base type filters (e.g. on <c>BaseRequest</c>) will also apply to derived types.
    /// </summary>
    /// <param name="userContext">The GraphQL user context, typically <c>new Dictionary&lt;string, object?&gt;()</c> in tests.</param>
    /// <param name="data">The <typeparamref name="TDbContext"/> instance.</param>
    /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
    /// <param name="item">The entity instance to evaluate. Navigation properties used by filter projections must be loaded.</param>
    /// <returns><c>true</c> if all matching filters allow the entity; <c>false</c> if any filter excludes it.</returns>
    /// <example>
    /// <code>
    /// var filters = new Filters&lt;MyDbContext&gt;();
    /// MyEntityFilters.AddFilters(filters);
    ///
    /// var entity = await dbContext.FindAsync(entityType, entityId);
    ///
    /// // Load reference navigations needed by filter projections
    /// var entry = dbContext.Entry(entity);
    /// foreach (var nav in entry.Navigations)
    /// {
    ///     if (nav.Metadata is INavigation { IsCollection: false } &amp;&amp; !nav.IsLoaded)
    ///     {
    ///         await nav.LoadAsync();
    ///     }
    /// }
    ///
    /// var allowed = await filters.ShouldInclude(userContext, dbContext, user, entity);
    /// </code>
    /// </example>
    #region ShouldIncludeSignature

    public async Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? user,
        object item)

    #endregion

    {
        if (entries.Count == 0)
        {
            return true;
        }

        var filterEntries = GetFilters(item.GetType()).ToList();
        if (filterEntries.Count == 0)
        {
            return true;
        }

        foreach (var entry in filterEntries)
        {
            if (!await entry.ShouldIncludeWithProjection(userContext, data, user, item))
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
