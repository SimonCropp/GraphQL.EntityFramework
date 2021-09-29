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
        object? GetArguments(Type type, string x) => context.GetArgument(type, x);

        if (keyNames is not null)
        {
            if (ArgumentReader.TryReadIds(GetArguments, out var values))
            {
                var keyName = GetKeyName(keyNames);
                var predicate = ExpressionBuilder<TItem>.BuildPredicate(keyName, Comparison.In, values);
                queryable = queryable.Where(predicate);
            }

            if (ArgumentReader.TryReadId(GetArguments, out var value))
            {
                var keyName = GetKeyName(keyNames);
                var predicate = ExpressionBuilder<TItem>.BuildPredicate(keyName, Comparison.Equal, new[] {value});
                queryable = queryable.Where(predicate);
            }
        }

        if (ArgumentReader.TryReadWhere(GetArguments, out var wheres))
        {
            var predicate = ExpressionBuilder<TItem>.BuildPredicate(wheres);
            queryable = queryable.Where(predicate);
        }

        if (applyOrder)
        {
            queryable = Order(queryable, GetArguments);

            if (ArgumentReader.TryReadSkip(GetArguments, out var skip))
            {
                queryable = queryable.Skip(skip);
            }

            if (ArgumentReader.TryReadTake(GetArguments, out var take))
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

    static IQueryable<TItem> Order<TItem>(IQueryable<TItem> queryable, Func<Type, string, object?> getArguments)
    {
        var orderBys = ArgumentReader.ReadOrderBy(getArguments).ToList();
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