namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(this IEnumerable<TItem> items, bool hasId, IResolveFieldContext context)
    {
        object? GetArguments(Type type, string name) => context.GetArgument(type, name);

        if (hasId)
        {
            if (ArgumentReader.TryReadIds(GetArguments, out var values))
            {
                var predicate = ExpressionBuilder<TItem>.BuildPredicate("Id", Comparison.In, values);
                items = items.Where(predicate.Compile());
            }
        }

        if (ArgumentReader.TryReadWhere(GetArguments, out var wheres))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(wheres);
            items = items.Where(predicate.Compile());
        }

        items = Order(items, GetArguments);

        if (ArgumentReader.TryReadSkip(GetArguments, out var skip))
        {
            items = items.Skip(skip);
        }

        if (ArgumentReader.TryReadTake(GetArguments, out var take))
        {
            items = items.Take(take);
        }

        return items;
    }

    static IEnumerable<TItem> Order<TItem>(IEnumerable<TItem> queryable, Func<Type, string, object?> getArguments)
    {
        var items = queryable.ToList();
        var orderBys = ArgumentReader.ReadOrderBy(getArguments).ToList();
        IOrderedEnumerable<TItem> ordered;
        if (orderBys.Count > 0)
        {
            var orderBy = orderBys.First();
            var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
            if (orderBy.Descending)
            {
                ordered = items.OrderByDescending(propertyFunc);
            }
            else
            {
                ordered = items.OrderBy(propertyFunc);
            }
        }
        else
        {
            return items;
        }

        foreach (var orderBy in orderBys.Skip(1))
        {
            var propertyFunc = PropertyCache<TItem>.GetProperty(orderBy.Path).Func;
            if (orderBy.Descending)
            {
                ordered = ordered.ThenByDescending(propertyFunc);
            }
            else
            {
                ordered = ordered.ThenBy(propertyFunc);
            }
        }

        return ordered;
    }
}