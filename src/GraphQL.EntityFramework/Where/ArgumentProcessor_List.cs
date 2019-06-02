using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ArgumentProcessor
    {
        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IEnumerable<TItem> items, ResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(items), items);
            Guard.AgainstNull(nameof(context), context);
            return ApplyToAll(items, (type, x) => context.GetArgument(type, x));
        }

        static IEnumerable<TItem> ApplyToAll<TItem>(this IEnumerable<TItem> items, Func<Type, string, object> getArguments)
        {
            if (ArgumentReader.TryReadIds(getArguments, out var values))
            {
                var predicate = FuncBuilder<TItem>.BuildPredicate("Id", Comparison.In, values);
                items = items.Where(predicate);
            }

            foreach (var where in ArgumentReader.ReadWhere(getArguments))
            {
                var predicate = FuncBuilder<TItem>.BuildPredicate(where);
                items = items.Where(predicate);
            }

            items = Order(items, getArguments);

            if (ArgumentReader.TryReadSkip(getArguments, out var skip))
            {
                items = items.Skip(skip);
            }

            if (ArgumentReader.TryReadTake(getArguments, out var take))
            {
                items = items.Take(take);
            }

            return items;
        }

        static IEnumerable<TItem> Order<TItem>(IEnumerable<TItem> queryable, Func<Type, string, object> getArguments)
        {
            var items = queryable.ToList();
            var orderBys = ArgumentReader.ReadOrderBy(getArguments).ToList();
            IOrderedEnumerable<TItem> ordered;
            if (orderBys.Count > 0)
            {
                var orderBy = orderBys.First();
                var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
                if (orderBy.Descending)
                {
                    ordered = items.OrderByDescending(propertyFunc);
                }
                else
                {
                    ordered = items.OrderBy(propertyFunc);
                }
            }
            else
            {
                return items;
            }

            foreach (var orderBy in orderBys.Skip(1))
            {
                var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
                if (orderBy.Descending)
                {
                    ordered = ordered.ThenByDescending(propertyFunc);
                }
                else
                {
                    ordered = ordered.ThenBy(propertyFunc);
                }
            }

            return ordered;
        }
    }
}