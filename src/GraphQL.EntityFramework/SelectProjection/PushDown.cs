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
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        var key = context.ExecutionContext;
        if (fields.Count == 0 ||
            key is null)
        {
            return;
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        var set = applied.GetValue(key, _ => new());
        foreach (var field in fields)
        {
            set[field] = 0;
        }
    }

    public static bool IsApplied(IResolveFieldContext context)
    {
        // A context built by hand, outside an execution, has no request to look up and no ast
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        var key = context.ExecutionContext;
        var field = context.FieldAst;
        if (key is null ||
            field is null)
        {
            return false;
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        return applied.TryGetValue(key, out var set) &&
               set.ContainsKey(field);
    }
}
