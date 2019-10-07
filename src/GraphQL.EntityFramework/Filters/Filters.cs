using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    #region FiltersSignature

    public class Filters
    {
        public delegate bool Filter<in TEntity>(object userContext, TEntity input)
            where TEntity : class;

        public delegate Task<bool> AsyncFilter<in TEntity>(object userContext, TEntity input)
            where TEntity : class;

        #endregion

        public void Add<TEntity>(Filter<TEntity> filter) where TEntity : class
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(TEntity)] =
                (context, item) =>
                {
                    try
                    {
                        return Task.FromResult(filter(context, (TEntity) item));
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                    }
                };
        }

        public void Add<TEntity>(AsyncFilter<TEntity> filter)
            where TEntity : class
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(TEntity)] =
                async (context, item) =>
                {
                    try
                    {
                        return await filter(context, (TEntity) item);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
                    }
                };
        }

        Dictionary<Type, Func<object, object, Task<bool>>> funcs = new Dictionary<Type, Func<object, object, Task<bool>>>();

        internal virtual async Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext)
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
                if (await ShouldInclude(userContext, item, filters))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        static async Task<bool> ShouldInclude<TEntity>(object userContext, TEntity item, List<Func<object, TEntity, Task<bool>>> filters)
            where TEntity : class
        {
            if (item == null)
            {
                return false;
            }

            foreach (var func in filters)
            {
                if (!await func(userContext, item))
                {
                    return false;
                }
            }

            return true;
        }

        internal virtual async Task<bool> ShouldInclude<TEntity>(object userContext, TEntity? item)
            where TEntity : class
        {
            if (item == null)
            {
                return false;
            }

            if (funcs.Count == 0)
            {
                return true;
            }

            foreach (var func in FindFilters<TEntity>())
            {
                if (!await func(userContext, item))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<Func<object, TEntity, Task<bool>>> FindFilters<TEntity>()
            where TEntity : class
        {
            var type = typeof(TEntity);
            foreach (var pair in funcs.Where(x => x.Key.IsAssignableFrom(type)))
            {
                yield return (context, item) => pair.Value(context, item);
            }
        }
    }
}