/// <summary>
/// Which navigation fields had their ids, where and orderBy applied inside the query that loaded
/// the parent, recorded per request so the field's own resolver applies only skip and take. The
/// resolver cannot tell from the collection alone, and a parent loaded some other way, such as by
/// a custom resolver, still needs the full in memory pass. Keyed on the execution context, which
/// is per request, since with document caching the ast nodes are shared between requests.
/// </summary>
static class PushDown
{
    static ConditionalWeakTable<object, ConcurrentDictionary<GraphQLField, byte>> applied = new();

    public static void Mark(IResolveFieldContext context, IReadOnlyList<GraphQLField> fields)
    {
        if (fields.Count == 0)
        {
            return;
        }

        var key = Key(context);
        if (key is null)
        {
            return;
        }

        var set = applied.GetValue(key, _ => new());
        foreach (var field in fields)
        {
            set[field] = 0;
        }
    }

    public static bool IsApplied(IResolveFieldContext context)
    {
        var key = Key(context);
        return key is not null &&
               applied.TryGetValue(key, out var set) &&
               set.ContainsKey(context.FieldAst);
    }

    static object? Key(IResolveFieldContext context) =>
        context.ExecutionContext;
}
