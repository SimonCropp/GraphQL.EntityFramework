using System;
using System.Linq;
using GraphQL.Types;

static partial class ArgumentProcessor
{
    public static IQueryable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IQueryable<TItem> queryable, ResolveFieldContext<TSource> context)
    {
        return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
    }

    static IQueryable<TItem> ApplyToAll<TItem>(this IQueryable<TItem> queryable, Func<Type, string, object> getArguments)
    {
        foreach (var where in ExpressionContextExtractor.ReadWhere<TItem>(getArguments))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(where);
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