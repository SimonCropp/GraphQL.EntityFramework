namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IQueryable<TItem> ApplyGraphQlArguments<TItem>(
        this IQueryable<TItem> queryable,
        IResolveFieldContext context,
        List<string>? keyNames,
        bool applyOrder,
        bool omitQueryArguments)
        where TItem : class
    {
        if (omitQueryArguments)
        {
            return queryable;
        }

        if (keyNames is not null)
        {
            if (ArgumentReader.TryReadIds(context, out var idValues))
            {
                var keyName = GetKeyName(keyNames);
                var predicate = ExpressionBuilder<TItem>.BuildIdPredicate(keyName, idValues);
                queryable = queryable.Where(predicate);
            }
        }

        if (ArgumentReader.TryReadWhere(context, out var wheres))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(wheres);
            queryable = queryable.Where(predicate);
        }

        if (applyOrder)
        {
            var (orderedItems, order) = Order(queryable, context);
            queryable = orderedItems;

            if (ArgumentReader.TryReadSkip(context, out var skip))
            {
                EnsureOrderForSkip(order, context);

                queryable = queryable.Skip(skip);
            }

            if (ArgumentReader.TryReadTake(context, out var take))
            {
                EnsureOrderForTake(order, context);

                queryable = queryable.Take(take);
            }
        }

        return queryable;
    }

    static (IQueryable<TItem> items, bool order) Order<TItem>(IQueryable<TItem> queryable, IResolveFieldContext context)
    {
        var orderBys = ArgumentReader.ReadOrderBy(context);
        IOrderedQueryable<TItem> ordered;
        if (orderBys.Count == 0)
        {
            return (queryable, false);
        }

        var orderBy = orderBys.First();
        var property = PropertyCache<TItem>.GetProperty(orderBy.Path)
            .Lambda;
        if (orderBy.Descending)
        {
            ordered = queryable.OrderByDescending(property);
        }
        else
        {
            ordered = queryable.OrderBy(property);
        }

        foreach (var subsequentOrderBy in orderBys.Skip(1))
        {
            var subsequentPropertyFunc = PropertyCache<TItem>.GetProperty(subsequentOrderBy.Path).Lambda;
            if (subsequentOrderBy.Descending)
            {
                ordered = ordered.ThenByDescending(subsequentPropertyFunc);
            }
            else
            {
                ordered = ordered.ThenBy(subsequentPropertyFunc);
            }
        }

        return (ordered, true);
    }
}