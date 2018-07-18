using System;
using System.Linq;
using GraphQL.EntityFramework;
using GraphQL.Types;

static partial class ArgumentProcessor
{
    public static IQueryable<TItem> ApplyGraphQlArguments<TItem, TSource>(this IQueryable<TItem> queryable, ResolveFieldContext<TSource> context)
    {
        return ApplyToAll(queryable, (type, x) => context.GetArgument(type, x));
    }

    static IQueryable<TItem> ApplyToAll<TItem>(this IQueryable<TItem> queryable, Func<Type, string, object> getArguments)
    {
        if (ExpressionContextExtractor.TryReadId(getArguments, out var idExpression))
        {
            var member = idExpression.Member;
            if (member == null)
            {
                member = "Id";
            }
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(member, Comparison.In, idExpression.Value);
            queryable = queryable.Where(predicate);
        }
        foreach (var where in ExpressionContextExtractor.ReadWhere(getArguments))
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