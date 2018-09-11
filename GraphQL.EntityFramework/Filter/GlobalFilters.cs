using System;
using System.Collections.Generic;

namespace GraphQL.EntityFramework
{
    public static class GlobalFilters
    {
        static Dictionary<Type, Func<object, object, bool>> funcs = new Dictionary<Type, Func<object, object, bool>>();

        public static void Add<TItem>(Filter<TItem> filter)
        {
            funcs[typeof(TItem)] = (context, item) => filter(context, (TItem)item);
        }

        public static bool ShouldInclude(object userContext, object item)
        {
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