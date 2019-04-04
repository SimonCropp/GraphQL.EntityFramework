using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    #region GlobalFiltersSignature

    public class GlobalFilters
    {
        public delegate bool Filter<in T>(object userContext, T input);

        public delegate Task<bool> AsyncFilter<in T>(object userContext, T input);

        #endregion

        public void Add<T>(Filter<T> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(T)] =
                (context, item) =>
                {
                    try
                    {
                        return Task.FromResult(filter(context, (T) item));
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. T: {typeof(T)}.", exception);
                    }
                };
        }

        public void Add<T>(AsyncFilter<T> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(T)] =
                async (context, item) =>
                {
                    try
                    {
                        return await filter(context, (T) item);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. T: {typeof(T)}.", exception);
                    }
                };
        }

        Dictionary<Type, Func<object, object, Task<bool>>> funcs = new Dictionary<Type, Func<object, object, Task<bool>>>();

        //internal async Task<List<T>> TryApplyFilter<T>(IQueryable<T> input, object userContext, CancellationToken token)
        //{
        //    if (funcs.Count == 0)
        //    {
        //        return null;
        //    }

        //    var filters = FindFilters<T>().ToList();
        //    if (filters.Count == 0)
        //    {
        //        return null;
        //    }

        //    var list = await input.ToListAsync(token);
        //    return list.Where(item =>
        //        {
        //            if (item == null)
        //            {
        //                return false;
        //            }

        //            return filters.All(func => func(userContext, item));
        //        })
        //        .ToList();
        //}

        internal async Task<IEnumerable<T>> ApplyFilter<T>(IEnumerable<T> result, object userContext)
        {
            if (funcs.Count == 0)
            {
                return result;
            }

            var filters = FindFilters<T>().ToList();
            if (filters.Count == 0)
            {
                return result;
            }

            var list = new List<T>();
            foreach (var item in result)
            {
                if (await ShouldInclude(userContext, item, filters))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        static async Task<bool> ShouldInclude<T>(object userContext, T item, List<Func<object, T, Task<bool>>> filters)
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

        internal async Task<bool> ShouldInclude<T>(object userContext, T item)
        {
            if (item == null)
            {
                return false;
            }

            if (funcs.Count == 0)
            {
                return true;
            }

            foreach (var func in FindFilters<T>())
            {
                if (!await func(userContext, item))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<Func<object, T, Task<bool>>> FindFilters<T>()
        {
            var type = typeof(T);
            foreach (var pair in funcs.Where(x => x.Key.IsAssignableFrom(type)))
            {
                yield return (context, item) => pair.Value(context, item);
            }
        }
    }
}