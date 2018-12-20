using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.EntityFramework
{
    public static class GlobalFilters
    {
        static Dictionary<Type, Func<object, object, bool>> funcs = new Dictionary<Type, Func<object, object, bool>>();

        public static void Clear()
        {
            funcs.Clear();
        }

        public static void Add<T>(Filter<T> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(T)] = (context, item) =>
            {
                try
                {
                    return filter(context, (T) item);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Failed to execute filter. TItem: {typeof(T)}.", exception);
                }
            };
        }

        internal static IEnumerable<T> ApplyFilter<T>(IEnumerable<T> result, object userContext)
        {
            if (funcs.Count <= 0)
            {
                return result;
            }
            var filters = FindFilters<T>();
            return result.Where(item =>
            {
                if (item == null)
                {
                    return false;
                }
                return filters.All(func => func(userContext, item));
            });
        }

        internal static bool ShouldInclude<T>(object userContext, T item)
        {
            if (item == null)
            {
                return false;
            }

            return FindFilters<T>().All(func => func(userContext, item));
        }

        internal static IEnumerable<Func<object, T, bool>> FindFilters<T>()
        {
            var type = typeof(T);
            foreach (var pair in funcs)
            {
                if (pair.Key.IsAssignableFrom(type))
                {
                    var func = pair.Value;
                    yield return (context, item) => func(context, item) ;
                }
            }
        }
    }
}