using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentProcessor
    {
        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IEnumerable<TItem> queryable, ResolveFieldContext<TSource> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> queryable, ResolveFieldContext context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> queryable, ResolveFieldContext<object> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        static IEnumerable<TItem> ApplyToAll<TItem>(this IEnumerable<TItem> queryable, Func<Type, string, object> getArguments)
        {
            foreach (var where in ExpressionContextExtractor.Read<TItem>(getArguments))
            {
                var predicate = FuncBuilder<TItem>.BuildPredicate(where);
                queryable = queryable.Where(predicate);
            }

            return queryable;
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
            foreach (var where in ExpressionContextExtractor.Read<TItem>(getArguments))
            {
                var predicate = ExpressionBuilder.BuildPredicate<TItem>(where);
                queryable = queryable.Where(predicate);
            }

            return queryable;
        }
    }
}