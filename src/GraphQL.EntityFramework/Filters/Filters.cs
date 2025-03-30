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
        funcs[typeof(TEntity)] =
            (userContext, data, userPrincipal, item) =>
            {
                try
                {
                    return Task.FromResult(filter(userContext, data, userPrincipal, (TEntity) item));
                }
                catch (Exception exception)
                {
                    throw new($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                }
            };

    public void Add<TEntity>(AsyncFilter<TEntity> filter)
        where TEntity : class =>
        funcs[typeof(TEntity)] =
            async (userContext, data, userPrincipal, item) =>
            {
                try
                {
                    return await filter(userContext, data, userPrincipal, (TEntity) item);
                }
                catch (Exception exception)
                {
                    throw new($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                }
            };

    delegate Task<bool> Filter(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, object input);

    Dictionary<Type, Filter> funcs = [];

    internal virtual async Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext, TDbContext data, ClaimsPrincipal? userPrincipal)
        where TEntity : class
    {
        if (funcs.Count == 0)
        {
            return result;
        }

        var filters = FindFilters<TEntity>().ToList();
        if (filters.Count == 0)
        {
            return result;
        }

        var list = new List<TEntity>();
        foreach (var item in result)
        {
            if (await ShouldInclude(userContext, data, userPrincipal, item, filters))
            {
                list.Add(item);
            }
        }

        return list;
    }

    static async Task<bool> ShouldInclude<TEntity>(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, TEntity item, List<AsyncFilter<TEntity>> filters)
        where TEntity : class
    {
        foreach (var func in filters)
        {
            if (!await func(userContext, data, userPrincipal, item))
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

        if (funcs.Count == 0)
        {
            return true;
        }

        foreach (var func in FindFilters<TEntity>())
        {
            if (!await func(userContext, data, userPrincipal, item))
            {
                return false;
            }
        }

        return true;
    }

    IEnumerable<AsyncFilter<TEntity>> FindFilters<TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        foreach (var pair in funcs.Where(_ => _.Key.IsAssignableFrom(type)))
        {
            yield return (context, data, user, item) => pair.Value(context, data, user, item);
        }
    }
}