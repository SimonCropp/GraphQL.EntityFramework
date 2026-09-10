/// <summary>
/// The in memory predicate of a navigation list or connection field, compiled once per request.
/// The field's resolver runs once per parent row, and building and interpreting the ids and
/// where for every row cost about 4us a row. The arguments of a field are fixed for the request,
/// literal or from variables, so the predicate is the same for every row. Keyed on the execution
/// context, which is per request, since with document caching the ast nodes are shared between
/// requests; the entries are collected with it. The item type is part of the key, since a field
/// selected on an interface is one ast node resolved by each implementing type.
/// </summary>
static class PredicateCache
{
    static ConditionalWeakTable<object, ConcurrentDictionary<(GraphQLField Field, Type Item), object?>> predicates = new();

    public static Func<TItem, bool>? GetOrAdd<TItem>(
        IResolveFieldContext context,
        List<string>? keyNames,
        Func<List<string>?, IResolveFieldContext, Func<TItem, bool>?> build)
    {
        var key = context.ExecutionContextOrNull();
        var field = context.FieldAstOrNull();
        // A context built by hand, outside an execution, has no request to cache against
        if (key is null ||
            field is null)
        {
            return build(keyNames, context);
        }

        var forRequest = predicates.GetValue(key, _ => new());
        var cacheKey = (field, typeof(TItem));
        if (forRequest.TryGetValue(cacheKey, out var existing))
        {
            return (Func<TItem, bool>?) existing;
        }

        var predicate = build(keyNames, context);
        forRequest[cacheKey] = predicate;
        return predicate;
    }
}
