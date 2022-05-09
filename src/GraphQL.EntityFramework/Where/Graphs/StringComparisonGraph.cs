using GraphQL.Types;

class StringComparisonGraph :
    EnumerationGraphType
{
    public StringComparisonGraph()
    {
        Name = nameof(StringComparison);
        Add("currentCulture", StringComparison.CurrentCulture);
        Add("currentCultureIgnoreCase", StringComparison.CurrentCultureIgnoreCase);
        Add("invariantCulture", StringComparison.InvariantCulture);
        Add("invariantCultureIgnoreCase", StringComparison.InvariantCultureIgnoreCase);
        Add("ordinal", StringComparison.Ordinal);
        Add("ordinalIgnoreCase", StringComparison.OrdinalIgnoreCase);
    }
}
