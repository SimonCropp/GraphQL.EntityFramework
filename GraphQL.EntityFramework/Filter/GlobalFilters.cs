using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    public static class GlobalFilters
    {
        static Dictionary<Type, Func<object,CancellationToken,Task<Func<object,bool>>>> funcs = new Dictionary<Type, Func<object, CancellationToken, Task<Func<object, bool>>>>();

        public static void Add<TItem>(FilterBuilder<TItem> filterBuilder)
        {
            funcs[typeof(TItem)] = async (userContext, token) =>
            {
                var filter = await filterBuilder(userContext, token);
                return item => filter((TItem) item);
            };
        }

        internal static Task<Func<object, bool>> GetFilter<TItem>(object userContext, CancellationToken token = default)
        {
            var itemType = typeof(TItem);
            foreach (var pair in funcs)
            {
                if (!pair.Key.IsAssignableFrom(itemType))
                {
                    continue;
                }

                var filterBuilder = pair.Value;
                return filterBuilder(userContext, token);
            }

            return Task.FromResult((Func<object, bool>)null);
        }

    }
}