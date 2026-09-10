namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(
        this IEnumerable<TItem> items,
        bool hasId,
        IResolveFieldContext context,
        bool omitQueryArguments) =>
        items.ApplyGraphQlArguments(hasId ? ["Id"] : null, context, omitQueryArguments);

    /// <summary>
    /// The key name comes from the model, the same as on the queryable path. The overload above
    /// assumed the key is named Id, so an entity keyed by anything else failed on the ids argument.
    /// </summary>
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(
        this IEnumerable<TItem> items,
        List<string>? keyNames,
        IResolveFieldContext context,
        bool omitQueryArguments) =>
        items.ApplyGraphQlArguments(keyNames, context, omitQueryArguments, false);

    /// <param name="argumentsAppliedInQuery">
    /// The ids, where and orderBy were applied inside the query that loaded the collection, so
    /// only skip and take remain. Applying the rest again would page twice, and would evaluate the
    /// where under the in memory comparison rules after the database already applied its own.
    /// </param>
    public static IEnumerable<TItem> ApplyGraphQlArguments<TItem>(
        this IEnumerable<TItem> items,
        List<string>? keyNames,
        IResolveFieldContext context,
        bool omitQueryArguments,
        bool argumentsAppliedInQuery)
    {
        // A field selected without arguments has nothing to apply. Reading them anyway made
        // GraphQL.NET build the argument dictionary, which it does lazily, per field per row.
        if (omitQueryArguments ||
            !ArgumentReader.HasArguments(context))
        {
            return items;
        }

        var alreadyOrdered = items is ICollection<TItem>;
        var order = argumentsAppliedInQuery;

        if (!argumentsAppliedInQuery)
        {
            var predicate = PredicateCache.GetOrAdd(context, keyNames, static (keyNames, context) => BuildPredicate<TItem>(keyNames, context));
            if (predicate is not null)
            {
                items = items.Where(predicate);
            }

            (items, order) = Order(items, context);
        }

        if (ArgumentReader.TryReadSkip(context, out var skip))
        {
            EnsureOrderForSkip(order || alreadyOrdered, context);

            items = items.Skip(skip);
        }

        if (ArgumentReader.TryReadTake(context, out var take))
        {
            EnsureOrderForTake(order || alreadyOrdered, context);

            items = items.Take(take);
        }

        return items;
    }

    /// <summary>
    /// The ids and where of the field as one in memory predicate, or null when it has neither.
    /// Built once per request per field, through <see cref="PredicateCache"/>, since the
    /// arguments are the same for every parent row.
    /// </summary>
    static Func<TItem, bool>? BuildPredicate<TItem>(List<string>? keyNames, IResolveFieldContext context)
    {
        Func<TItem, bool>? ids = null;
        if (keyNames is not null &&
            ArgumentReader.TryReadIds(context, out var idValues))
        {
            var keyName = GetKeyName(keyNames);
            ids = Compile(ExpressionBuilder<TItem>.BuildIdPredicate(keyName, idValues));
        }

        Func<TItem, bool>? where = null;
        if (ArgumentReader.TryReadWhere(context, out var wheres))
        {
            where = Compile(ExpressionBuilder<TItem>.BuildPredicate(wheres));
        }

        if (ids is null)
        {
            return where;
        }

        if (where is null)
        {
            return ids;
        }

        return _ => ids(_) && where(_);
    }

    /// <summary>
    /// Emitting IL costs ~700us a call and leaves behind a DynamicMethod that is never collected,
    /// which dwarfs the cost of running the predicate over an in memory collection. Interpreting
    /// is ~20x cheaper to construct and stays ahead until a collection reaches several thousand items.
    /// </summary>
    static Func<TItem, bool> Compile<TItem>(Expression<Func<TItem, bool>> predicate) =>
        InMemoryStringComparison.Rewrite(predicate).Compile(preferInterpretation: true);

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