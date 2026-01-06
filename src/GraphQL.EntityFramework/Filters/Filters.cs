namespace GraphQL.EntityFramework;

#region FiltersSignature

public class Filters<TDbContext>
    where TDbContext : DbContext
{
    public delegate bool Filter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input)
        where TEntity : class;

    public delegate Task<bool> AsyncFilter<in TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity input)
        where TEntity : class;

    #endregion

    public void Add<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        AsyncFilter<TProjection> filter)
        where TEntity : class
        where TProjection : class =>
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
        where TEntity : class
        where TProjection : class =>
        entries[typeof(TEntity)] = new FilterEntry<TDbContext, TEntity, TProjection>(
            async (userContext, data, userPrincipal, item) =>
            {
                try
                {
                    return filter(userContext, data, userPrincipal, item);
                }
                catch (Exception exception)
                {
                    throw new($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                }
            },
            projection);

    delegate Task<bool> Filter(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, object input);

    Dictionary<Type, IFilterEntry<TDbContext>> entries = [];

    internal virtual async Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext, TDbContext data, ClaimsPrincipal? userPrincipal)
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

        // Query projected data if any filters need it
        var projectedDataMap = await QueryProjectedData(result, data, filterEntries);

        var list = new List<TEntity>();
        foreach (var item in result)
        {
            if (await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries, projectedDataMap))
            {
                list.Add(item);
            }
        }

        return list;
    }

    static async Task<Dictionary<(Type, object), object>> QueryProjectedData<TEntity>(
        IEnumerable<TEntity> entities,
        TDbContext data,
        List<IFilterEntry<TDbContext>> filterEntries)
        where TEntity : class
    {
        var projectedDataMap = new Dictionary<(Type, object), object>();

        foreach (var filter in filterEntries)
        {
            var projected = await filter.QueryProjectedData(entities, data);
            foreach (var (id, projectedItem) in projected)
            {
                projectedDataMap[(filter.EntityType, id)] = projectedItem;
            }
        }

        return projectedDataMap;
    }

    static async Task<Dictionary<(Type, object), object>> QueryProjectedDataForSingle<TEntity>(
        TEntity entity,
        TDbContext data,
        List<IFilterEntry<TDbContext>> filterEntries)
        where TEntity : class
    {
        var projectedDataMap = new Dictionary<(Type, object), object>();
        var idProperty = typeof(TEntity).GetProperty("Id")!;
        var id = idProperty.GetValue(entity)!;

        foreach (var filter in filterEntries)
        {
            var projectedItem = await filter.QueryProjectedDataForSingle(entity, data);
            if (projectedItem != null)
            {
                projectedDataMap[(filter.EntityType, id)] = projectedItem;
            }
        }

        return projectedDataMap;
    }

    static async Task<bool> ShouldIncludeItem<TEntity>(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        TEntity item,
        List<IFilterEntry<TDbContext>> filterEntries,
        Dictionary<(Type, object), object> projectedDataMap)
        where TEntity : class
    {
        foreach (var entry in filterEntries)
        {
            if (!await entry.ShouldInclude(userContext, data, userPrincipal, item, projectedDataMap))
            {
                return false;
            }
        }

        return true;
    }

    internal virtual async Task<bool> ShouldInclude<TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity? item)
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

        // Query projected data if needed
        var projectedDataMap = await QueryProjectedDataForSingle(item, data, filterEntries);

        return await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries, projectedDataMap);
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
