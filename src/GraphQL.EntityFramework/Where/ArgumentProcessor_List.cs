namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(
        this IEnumerable<TItem> items,
        bool hasId,
        IResolveFieldContext context,
        bool omitQueryArguments)
    {
        if (omitQueryArguments)
        {
            return items;
        }

        var alreadyOrdered = items is ICollection<TItem>;

        if (hasId)
        {
            if (ArgumentReader.TryReadIds(context, out var idValues))
            {
                var predicate = ExpressionBuilder<TItem>.BuildIdPredicate("Id", idValues);
                items = items.Where(Compile(predicate));
            }
        }

        if (ArgumentReader.TryReadWhere(context, out var wheres))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(wheres);
            items = items.Where(Compile(predicate));
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

    /// <summary>
    /// This path runs once per parent node, so a navigation list field is compiled as many times as there
    /// are parents. Emitting IL costs ~700us a call and leaves behind a DynamicMethod that is never
    /// collected, which dwarfs the cost of running the predicate over an in memory collection. Interpreting
    /// is ~20x cheaper to construct and stays ahead until a collection reaches several thousand items.
    /// </summary>
    static Func<TItem, bool> Compile<TItem>(Expression<Func<TItem, bool>> predicate) =>
        predicate.Compile(preferInterpretation: true);

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