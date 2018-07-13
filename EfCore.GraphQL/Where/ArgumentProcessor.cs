using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentProcessor
    {
        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IEnumerable<TItem> items, ResolveFieldContext<TSource> context)
        {
            return ApplyToAll(items, (type, x) => context.GetArgument(type, x));
        }

        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> items, ResolveFieldContext context)
        {
            return ApplyToAll(items, (type, x) => context.GetArgument(type, x));
        }

        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> items, ResolveFieldContext<object> context)
        {
            return ApplyToAll(items, (type, x) => context.GetArgument(type, x));
        }

        static IEnumerable<TItem> ApplyToAll<TItem>(this IEnumerable<TItem> items, Func<Type, string, object> getArguments)
        {
            foreach (var where in ExpressionContextExtractor.ReadWhere<TItem>(getArguments))
            {
                var predicate = FuncBuilder<TItem>.BuildPredicate(where);
                items = items.Where(predicate);
            }

            if (ExpressionContextExtractor.TryReadSkip(getArguments,out var skip))
            {
                items = items.Skip(skip);
            }

            if (ExpressionContextExtractor.TryReadTake(getArguments,  out var take))
            {
                items = items.Take(take);
            }
            return items;
        }

        public static IQueryable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IQueryable<TItem> queryable, ResolveFieldContext<TSource> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IQueryable<TItem> ApplyGraphQlArguments<TItem>(this IQueryable<TItem> queryable, ResolveFieldContext context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IQueryable<TItem> ApplyGraphQlArguments<TItem>(this IQueryable<TItem> queryable, ResolveFieldContext<object> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        static IQueryable<TItem> ApplyToAll<TItem>(this IQueryable<TItem> queryable, Func<Type, string, object> getArguments)
        {
            foreach (var where in ExpressionContextExtractor.ReadWhere<TItem>(getArguments))
            {
                var predicate = ExpressionBuilder.BuildPredicate<TItem>(where);
                queryable = queryable.Where(predicate);
            }
            if (ExpressionContextExtractor.TryReadSkip(getArguments, out var skip))
            {
                queryable = queryable.Skip(skip);
            }

            if (ExpressionContextExtractor.TryReadTake(getArguments, out var take))
            {
                queryable = queryable.Take(take);
            }

            return queryable;
        }
    }
}