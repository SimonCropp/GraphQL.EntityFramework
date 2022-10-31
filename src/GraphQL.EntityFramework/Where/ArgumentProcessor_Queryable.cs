namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IQueryable<TItem> ApplyGraphQlArguments<TItem>(
        this IQueryable<TItem> queryable,
        IResolveFieldContext context,
        List<string>? keyNames,
        bool applyOrder)
        where TItem : class
    {
        if (keyNames is not null)
        {
            if (ArgumentReader.TryReadIds(context, out var values))
            {
                var keyName = GetKeyName(keyNames);
                var predicate = ExpressionBuilder<TItem>.BuildPredicate(keyName, Comparison.In, values);
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
            queryable = Order(queryable, context);

            if (ArgumentReader.TryReadSkip(context, out var skip))
            {
                queryable = queryable.Skip(skip);
            }

            if (ArgumentReader.TryReadTake(context, out var take))
            {
                queryable = queryable.Take(take);
            }
        }

        return queryable;
    }

    static string GetKeyName(List<string> keyNames)
    {
        if (keyNames.Count > 1)
        {
            throw new("Only one id field is currently supported");
        }

        return keyNames[0];
    }

    static IQueryable<TItem> Order<TItem>(IQueryable<TItem> queryable, IResolveFieldContext context)
    {
        var orderBys = ArgumentReader.ReadOrderBy(context).ToList();
        IOrderedQueryable<TItem> ordered;
        if (orderBys.Count > 0)
        {
            var orderBy = orderBys.First();
            var property = PropertyCache<TItem>.GetProperty(orderBy.Path).Lambda;
            if (orderBy.Descending)
            {
                ordered = queryable.OrderByDescending(property);
            }
            else
            {
                ordered = queryable.OrderBy(property);
            }
        }
        else
        {
            return queryable;
        }

        foreach (var orderBy in orderBys.Skip(1))
        {
            var property = PropertyCache<TItem>.GetProperty(orderBy.Path).Lambda;
            if (orderBy.Descending)
            {
                ordered = ordered.ThenByDescending(property);
            }
            else
            {
                ordered = ordered.ThenBy(property);
            }
        }

        return ordered;
    }
}