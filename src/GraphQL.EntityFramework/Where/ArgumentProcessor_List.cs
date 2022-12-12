namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> items, bool hasId, IResolveFieldContext context)
    {
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

        items = Order(items, context);

        if (ArgumentReader.TryReadSkip(context, out var skip))
        {
            items = items.Skip(skip);
        }

        if (ArgumentReader.TryReadTake(context, out var take))
        {
            items = items.Take(take);
        }

        return items;
    }

    static IEnumerable<TItem> Order<TItem>(IEnumerable<TItem> queryable, IResolveFieldContext context)
    {
        var customSorting = context.RequestServices?.GetService<ICustomSorting<TItem>>();

        var items = queryable.ToList();
        var orderBys = ArgumentReader.ReadOrderBy(context).ToList();
        IOrderedEnumerable<TItem> ordered;
        if (orderBys is { Count: > 0 })
        {
            var orderBy = orderBys.First();
            if (!(customSorting?.ApplySort(items, orderBy, true, out ordered) ?? false))
            {
                var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
                ordered = orderBy.Descending ? items.OrderByDescending(propertyFunc) : items.OrderBy(propertyFunc);
            }
        }
        else
        {
            return items;
        }
        
        foreach (var orderBy in orderBys.Skip(1))
        {
            if (customSorting?.ApplySort(ordered, orderBy, false, out ordered) ?? false)
                continue;

            var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
            ordered = orderBy.Descending ? ordered.ThenByDescending(propertyFunc) : ordered.ThenBy(propertyFunc);
        }

        return ordered;
    }
}