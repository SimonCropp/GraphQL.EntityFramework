namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(
        this IEnumerable<TItem> items,
        bool hasId,
        IResolveFieldContext context,
        bool omitQueryArguments = false)
    {
        if (omitQueryArguments)
        {
            return items;
        }

        var alreadyOrdered = items is ICollection<TItem>;

        if (hasId)
        {
            if (ArgumentReader.TryReadIds(context, out var values))
            {
                var predicate = ExpressionBuilder<TItem>.BuildPredicate("Id", Comparison.In, values);
                items = items.Where(predicate.Compile());
            }
        }

        if (ArgumentReader.TryReadWhere(context, out var wheres))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(wheres);
            items = items.Where(predicate.Compile());
        }

        var (orderedItems, order) = Order(items, context);
        items = orderedItems;

        if (ArgumentReader.TryReadSkip(context, out var skip))
        {
            EnsureOrderForSkip(order|| alreadyOrdered, context);

            items = items.Skip(skip);
        }

        if (ArgumentReader.TryReadTake(context, out var take))
        {
            EnsureOrderForTake(order|| alreadyOrdered, context);

            items = items.Take(take);
        }

        return items;
    }

    static (IEnumerable<TItem> items, bool order) Order<TItem>(IEnumerable<TItem> queryable, IResolveFieldContext context)
    {
        var orderBys = ArgumentReader
            .ReadOrderBy(context);
        if (orderBys.Count == 0)
        {
            return (queryable, false);
        }

        IOrderedEnumerable<TItem> ordered;
        var orderBy = orderBys.First();
        var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path)
            .Func;
        if (orderBy.Descending)
        {
            ordered = queryable.OrderByDescending(propertyFunc);
        }
        else
        {
            ordered = queryable.OrderBy(propertyFunc);
        }

        foreach (var subsequentOrderBy in orderBys.Skip(1))
        {
            var subsequentPropertyFunc = PropertyCache<TItem>.GetProperty(subsequentOrderBy.Path)
                .Func;
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