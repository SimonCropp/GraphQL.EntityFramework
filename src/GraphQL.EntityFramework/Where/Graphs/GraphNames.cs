/// <summary>
/// The names of the generated input types. They follow the CLR type name, which is unambiguous
/// where the GraphQL scalar names are not: float and double both print as Float.
/// </summary>
static class GraphNames
{
    public static string Where(Type type) =>
        $"{Sanitise(type)}Where";

    public static string CollectionWhere(Type type) =>
        $"{Sanitise(type)}CollectionWhere";

    public static string Comparison(Type type) =>
        $"{Sanitise(type)}Comparison";

    public static string OrderBy(Type type) =>
        $"{Sanitise(type)}OrderBy";

    static string Sanitise(Type type)
    {
        var name = type.Name;
        // Generic arity, as in List`1
        var arity = name.IndexOf((char) 96);
        if (arity >= 0)
        {
            name = name[..arity];
        }

        var builder = new StringBuilder(name.Length);
        foreach (var character in name)
        {
            builder.Append(char.IsAsciiLetterOrDigit(character) || character == '_' ? character : '_');
        }

        return builder.ToString();
    }
}
