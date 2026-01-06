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

    public void Add<TEntity>(Filter<TEntity> filter)
        where TEntity : class =>
        entries[typeof(TEntity)] = new FilterEntry<TDbContext, TEntity>(
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
            projection: null);

    public void Add<TEntity>(AsyncFilter<TEntity> filter)
        where TEntity : class =>
        entries[typeof(TEntity)] = new FilterEntry<TDbContext, TEntity>(
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
            projection: null);

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

        foreach (var entry in filterEntries.Where(_ => _.HasProjection))
        {
            var projected = await entry.QueryProjectedData(entities, data);
            foreach (var (id, projectedItem) in projected)
            {
                projectedDataMap[(entry.EntityType, id)] = projectedItem;
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
        var projectedDataMap = await QueryProjectedData([item], data, filterEntries);

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

interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    Type EntityType { get; }
    bool HasProjection { get; }
    Task<Dictionary<object, object>> QueryProjectedData(IEnumerable<object> entities, TDbContext data);
    Task<bool> ShouldInclude(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, object item, Dictionary<(Type, object), object> projectedDataMap);
}

class FilterEntry<TDbContext, TEntity> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
{
    readonly Func<object, TDbContext, ClaimsPrincipal?, TEntity, Task<bool>> filter;

    public FilterEntry(Func<object, TDbContext, ClaimsPrincipal?, TEntity, Task<bool>> filter, object? projection) =>
        this.filter = filter;

    public Type EntityType => typeof(TEntity);
    public bool HasProjection => false;

    public Task<Dictionary<object, object>> QueryProjectedData(IEnumerable<object> entities, TDbContext data) =>
        Task.FromResult(new Dictionary<object, object>());

    public async Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item,
        Dictionary<(Type, object), object> projectedDataMap) =>
        await filter(userContext, data, userPrincipal, (TEntity) item);
}

class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
    where TProjection : class
{
    readonly Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    readonly Expression<Func<TEntity, TProjection>> projection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        this.projection = projection;
    }

    public Type EntityType => typeof(TEntity);
    public bool HasProjection => true;

    public async Task<Dictionary<object, object>> QueryProjectedData(IEnumerable<object> entities, TDbContext data)
    {
        var typedEntities = entities.Cast<TEntity>().ToList();
        if (typedEntities.Count == 0)
        {
            return [];
        }

        var idProperty = typeof(TEntity).GetProperty("Id")!;

        // Extract IDs using reflection to maintain the correct type
        var idsListType = typeof(List<>).MakeGenericType(idProperty.PropertyType);
        var idsList = (System.Collections.IList) Activator.CreateInstance(idsListType)!;

        foreach (var entity in typedEntities)
        {
            var id = idProperty.GetValue(entity);
            idsList.Add(id);
        }

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(parameter, idProperty);

        // Build: e => ids.Contains(e.Id)
        var idsConstant = Expression.Constant(idsList, idsListType);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(idProperty.PropertyType);
        var containsCall = Expression.Call(containsMethod, idsConstant, propertyAccess);
        var wherePredicate = Expression.Lambda(containsCall, parameter);

        // Query: data.Set<TEntity>().Where(e => ids.Contains(e.Id)).Select(projection)
        var query = data.Set<TEntity>()
            .Where((Expression<Func<TEntity, bool>>) wherePredicate)
            .Select(projection);

        var results = await query.ToListAsync();

        // Map results by ID
        var map = new Dictionary<object, object>();
        var projectedIdProperty = typeof(TProjection).GetProperty("Id")
            ?? throw new($"Projection type must include an 'Id' property.");

        foreach (var result in results)
        {
            var id = projectedIdProperty.GetValue(result)
                ?? throw new($"Projected Id was null for entity {typeof(TEntity).Name}");
            map[id] = result;
        }

        return map;
    }

    public async Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item,
        Dictionary<(Type, object), object> projectedDataMap)
    {
        var idProperty = typeof(TEntity).GetProperty("Id")!;
        var id = idProperty.GetValue(item)!;

        if (!projectedDataMap.TryGetValue((typeof(TEntity), id), out var projectedItem))
        {
            throw new($"Projected data not found for {typeof(TEntity).Name} with Id {id}");
        }

        return await filter(userContext, data, userPrincipal, (TProjection) projectedItem);
    }
}
