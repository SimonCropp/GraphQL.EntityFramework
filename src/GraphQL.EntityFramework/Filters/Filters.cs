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

    internal IReadOnlyDictionary<Type, IFilterEntry<TDbContext>> Entries => entries;

    internal IEnumerable<IFilterEntry<TDbContext>> GetFiltersForType(Type entityType) =>
        entries
            .Where(e => e.Key.IsAssignableFrom(entityType))
            .Select(e => e.Value);

    internal virtual async Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(
        IEnumerable<EntityWithFilterData<TEntity>> results,
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
        where TEntity : class
    {
        if (entries.Count == 0)
        {
            return results.Select(r => r.Entity);
        }

        var filterEntries = FindFilters<TEntity>().ToList();
        if (filterEntries.Count == 0)
        {
            return results.Select(r => r.Entity);
        }

        var list = new List<TEntity>();
        foreach (var item in results)
        {
            if (await ShouldIncludeItem(userContext, data, userPrincipal, item, filterEntries))
            {
                list.Add(item.Entity);
            }
        }

        return list;
    }

    static async Task<bool> ShouldIncludeItem<TEntity>(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        EntityWithFilterData<TEntity> item,
        List<IFilterEntry<TDbContext>> filterEntries)
        where TEntity : class
    {
        foreach (var entry in filterEntries)
        {
            if (!item.FilterData.TryGetValue(entry.EntityType, out var projectedData))
            {
                throw new($"Filter data not found for {entry.EntityType.Name}");
            }

            if (!await entry.ShouldInclude(userContext, data, userPrincipal, projectedData))
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
        EntityWithFilterData<TEntity>? item)
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

    // Overload for in-memory filtering (e.g., navigation properties where data is already loaded)
    internal virtual async Task<IEnumerable<TEntity>> ApplyFilterInMemory<TEntity>(
        IEnumerable<TEntity> results,
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
        where TEntity : class
    {
        if (entries.Count == 0)
        {
            return results;
        }

        var resultList = results.ToList();

        // First, filter any collection navigation properties on each entity
        foreach (var item in resultList)
        {
            await FilterNavigationProperties(item, userContext, data, userPrincipal);
        }

        // Then, filter the root entities themselves
        var filterEntries = FindFilters<TEntity>().ToList();
        if (filterEntries.Count == 0)
        {
            return resultList;
        }

        var list = new List<TEntity>();
        foreach (var item in resultList)
        {
            // Compute filter data by invoking projections in-memory
            var filterData = new Dictionary<Type, object>();
            foreach (var entry in filterEntries)
            {
                var projectedData = entry.ProjectInMemory(item);
                filterData[entry.EntityType] = projectedData;
            }

            var wrapped = new EntityWithFilterData<TEntity>
            {
                Entity = item,
                FilterData = filterData
            };

            if (await ShouldIncludeItem(userContext, data, userPrincipal, wrapped, filterEntries))
            {
                list.Add(item);
            }
        }

        return list;
    }

    async Task FilterNavigationProperties(
        object entity,
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal)
    {
        var entityType = entity.GetType();
        var properties = entityType.GetProperties();

        foreach (var property in properties)
        {
            // Check if this is a collection property
            if (!property.PropertyType.IsGenericType)
            {
                continue;
            }

            var genericDef = property.PropertyType.GetGenericTypeDefinition();
            if (genericDef != typeof(ICollection<>) &&
                genericDef != typeof(List<>) &&
                genericDef != typeof(IList<>))
            {
                continue;
            }

            var elementType = property.PropertyType.GetGenericArguments()[0];

            // Check if there's a filter for this element type
            if (!entries.TryGetValue(elementType, out var filterEntry))
            {
                continue;
            }

            // Get the collection
            var collection = property.GetValue(entity);
            if (collection is null)
            {
                continue;
            }

            // Filter the collection
            var filteredItems = new List<object>();
            foreach (var item in (System.Collections.IEnumerable)collection)
            {
                var projectedData = filterEntry.ProjectInMemory(item);
                if (await filterEntry.ShouldInclude(userContext, data, userPrincipal, projectedData))
                {
                    filteredItems.Add(item);
                }
            }

            // Replace collection contents
            var clearMethod = collection.GetType().GetMethod("Clear");
            var addMethod = collection.GetType().GetMethod("Add");
            if (clearMethod != null && addMethod != null)
            {
                clearMethod.Invoke(collection, null);
                foreach (var item in filteredItems)
                {
                    addMethod.Invoke(collection, [item]);
                }
            }
        }
    }

    internal virtual async Task<bool> ShouldIncludeInMemory<TEntity>(
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

        // Compute filter data by invoking projections in-memory
        var filterData = new Dictionary<Type, object>();
        foreach (var entry in filterEntries)
        {
            var projectedData = entry.ProjectInMemory(item);
            filterData[entry.EntityType] = projectedData;
        }

        var wrapped = new EntityWithFilterData<TEntity>
        {
            Entity = item,
            FilterData = filterData
        };

        return await ShouldIncludeItem(userContext, data, userPrincipal, wrapped, filterEntries);
    }
}
