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

        public static void Add<TItem>(Filter<TItem> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(TItem)] = (context, item) =>
            {
                try
                {
                    return filter(context, (TItem) item);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Failed to execute filter. TItem: {typeof(TItem)}.", exception);
                }
            };
        }

        internal static IEnumerable<TReturn> ApplyFilter<TReturn>(IEnumerable<TReturn> result, object userContext)
        {
            if (funcs.Count <= 0)
            {
                return result;
            }
            return result.Where(item => ShouldInclude(userContext, item));
        }

        internal static bool ShouldInclude(object userContext, object item)
        {
            if (item == null)
            {
                return false;
            }
            var itemType = item.GetType();
            foreach (var pair in funcs)
            {
                if (!pair.Key.IsAssignableFrom(itemType))
                {
                    continue;
                }
                if (!pair.Value(userContext, item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}