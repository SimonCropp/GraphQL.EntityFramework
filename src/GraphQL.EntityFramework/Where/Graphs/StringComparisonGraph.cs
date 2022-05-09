using GraphQL.Types;

class StringComparisonGraph :
    EnumerationGraphType
{
    public StringComparisonGraph()
    {
        Name = nameof(StringComparison);
        const string deprecation = "Use the camel case alternative";
        AddValue("currentCulture", null, StringComparison.CurrentCulture);
        AddValue("currentCultureIgnoreCase", null, StringComparison.CurrentCultureIgnoreCase);
        AddValue("invariantCulture", null, StringComparison.InvariantCulture);
        AddValue("invariantCultureIgnoreCase", null, StringComparison.InvariantCultureIgnoreCase);
        AddValue("ordinal", null, StringComparison.Ordinal);
        AddValue("ordinalIgnoreCase", null, StringComparison.OrdinalIgnoreCase);
    }
}
