namespace GraphQL.EntityFramework;

#region FiltersSignature

public partial class Filters<TDbContext>
    where TDbContext : DbContext
{
    public delegate bool Filter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

    public delegate Task<bool> AsyncFilter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input);

    #endregion

    public void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
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

    public void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
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

    public IReadOnlySet<string> GetRequiredFilterProperties<TEntity>()
        where TEntity : class
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filterEntries = FindFilters<TEntity>();

        foreach (var entry in filterEntries)
        {
            foreach (var prop in entry.RequiredPropertyNames)
            {
                result.Add(prop);
            }
        }

        return result;
    }

    public IReadOnlyDictionary<Type, IReadOnlySet<string>> GetAllRequiredFilterProperties()
    {
        var result = new Dictionary<Type, IReadOnlySet<string>>();

        foreach (var (entityType, entry) in entries)
        {
            var props = entry.RequiredPropertyNames;
            if (props.Count > 0)
            {
                result[entityType] = props;
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

        var filterEntries = FindFilters<TEntity>().ToList();
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

        var filterEntries = FindFilters<TEntity>().ToList();
        if (filterEntries.Count == 0)
        {
            return true;
        }

        return await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries);
    }

    List<IFilterEntry<TDbContext>> FindFilters<TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        return entries
            .Where(_ => _.Key.IsAssignableFrom(type))
            .Select(_ => _.Value)
            .ToList();
    }
}
