namespace GraphQL.EntityFramework;

public static partial class ArgumentProcessor
{
    static void EnsureOrderForTake(bool order, IResolveFieldContext context)
    {
        if (order)
        {
            return;
        }

        context.Errors.Add(new($"If `take` is used an `orderBy` must be specified. Field: {context.FieldDefinition.Name}"));
    }

    static void EnsureOrderForSkip(bool order, IResolveFieldContext context)
    {
        if (order)
        {
            return;
        }

        context.Errors.Add(new($"If `skip` is used an `orderBy` must be specified. Field: {context.FieldDefinition.Name}"));
    }

    static string GetKeyName(IReadOnlyList<string> keyNames)
    {
        if (keyNames.Count > 1)
        {
            throw new("Only one id field is currently supported");
        }

        return keyNames[0];
    }
}