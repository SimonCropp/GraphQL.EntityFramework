using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ArgumentProcessor
    {
        public static IQueryable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IQueryable<TItem> queryable, ResolveFieldContext<TSource> context, string primaryKeyName = "Id")
        {
            Guard.AgainstNull(nameof(queryable),queryable);
            Guard.AgainstNull(nameof(context),context);
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x), primaryKeyName);
        }

        static IQueryable<TItem> ApplyToAll<TItem>(this IQueryable<TItem> queryable, Func<Type, string, object> getArguments, string primaryKeyName)
        {
            if (ArgumentReader.TryReadIds(getArguments, out var values))
            {
                var predicate = ExpressionBuilder<TItem>.BuildPredicate(primaryKeyName, Comparison.In, values);
                queryable = queryable.Where(predicate);
            }

            if (ArgumentReader.TryReadId(getArguments, out var value))
            {
                var predicate = ExpressionBuilder<TItem>.BuildSinglePredicate(primaryKeyName, Comparison.Equal, value);
                queryable = queryable.Where(predicate);
            }

            foreach (var where in ArgumentReader.ReadWhere(getArguments))
            {
                var predicate = ExpressionBuilder<TItem>.BuildPredicate(where);
                queryable = queryable.Where(predicate);
            }

            queryable = Order(queryable, getArguments);

            if (ArgumentReader.TryReadSkip(getArguments, out var skip))
            {
                queryable = queryable.Skip(skip);
            }

            if (ArgumentReader.TryReadTake(getArguments, out var take))
            {
                queryable = queryable.Take(take);
            }

            return queryable;
        }

        static IQueryable<TItem> Order<TItem>(IQueryable<TItem> queryable, Func<Type, string, object> getArguments)
        {
            var orderBys = ArgumentReader.ReadOrderBy(getArguments).ToList();
            IOrderedQueryable<TItem> ordered;
            if (orderBys.Count > 0)
            {
                var orderBy = orderBys.First();
                var property = PropertyCache<TItem>.GetProperty(orderBy.Path).Lambda;
                if (orderBy.Descending)
                {
                    ordered = queryable.OrderByDescending(property);
                }
                else
                {
                    ordered = queryable.OrderBy(property);
                }
            }
            else
            {
                return queryable;
            }

            foreach (var orderBy in orderBys.Skip(1))
            {
                var property = PropertyCache<TItem>.GetProperty(orderBy.Path).Lambda;
                if (orderBy.Descending)
                {
                    ordered = ordered.ThenByDescending(property);
                }
                else
                {
                    ordered = ordered.ThenBy(property);
                }
            }

            return ordered;
        }
    }
}