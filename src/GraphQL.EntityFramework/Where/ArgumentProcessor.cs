namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    // Thrown rather than added to the errors, which let the query run and the response carry
    // nondeterministic data alongside the error
    static void EnsureOrderForTake(bool order, IResolveFieldContext context)
    {
        if (order)
        {
            return;
        }

        throw new($"If `take` is used an `orderBy` must be specified. Field: {context.FieldDefinition.Name}");
    }

    static void EnsureOrderForSkip(bool order, IResolveFieldContext context)
    {
        if (order)
        {
            return;
        }

        throw new($"If `skip` is used an `orderBy` must be specified. Field: {context.FieldDefinition.Name}");
    }

    internal static string GetKeyName(IReadOnlyList<string> keyNames)
    {
        if (keyNames.Count > 1)
        {
            throw new("Only one id field is currently supported");
        }

        return keyNames[0];
    }
}