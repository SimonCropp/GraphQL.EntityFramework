using System;
using System.Linq;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentProcessor
    {
        public static IQueryable<T> ApplyGraphQlArguments<T>(this IQueryable<T> queryable, ResolveFieldContext<T> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IQueryable<T> ApplyGraphQlArguments<T>(this IQueryable<T> queryable, ResolveFieldContext context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        public static IQueryable<T> ApplyGraphQlArguments<T>(this IQueryable<T> queryable, ResolveFieldContext<object> context)
        {
            return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
        }

        static IQueryable<T> ApplyToAll<T>(this IQueryable<T> queryable, Func<Type, string, object> getArguments)
        {
            foreach (var where in ExpressionContextExtractor.Read<T>(getArguments))
            {
                var predicate = ExpressionBuilder.BuildPredicate<T>(where);
                queryable = queryable.Where(predicate);
            }

            return queryable;
        }
    }
}