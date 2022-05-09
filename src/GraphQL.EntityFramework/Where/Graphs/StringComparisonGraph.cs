using GraphQL.Types;

class StringComparisonGraph :
    EnumerationGraphType
{
    public StringComparisonGraph()
    {
        Name = nameof(StringComparison);
        const string deprecation = "Use the camel case alternative";
        AddValue("currentCulture", null, StringComparison.CurrentCulture);
        AddValue("CURRENT_CULTURE", null, StringComparison.CurrentCulture, deprecation);
        AddValue("currentCultureIgnoreCase", null, StringComparison.CurrentCultureIgnoreCase);
        AddValue("CURRENT_CULTURE_IGNORE_CASE", null, StringComparison.CurrentCultureIgnoreCase, deprecation);
        AddValue("invariantCulture", null, StringComparison.InvariantCulture);
        AddValue("INVARIANT_CULTURE", null, StringComparison.InvariantCulture, deprecation);
        AddValue("invariantCultureIgnoreCase", null, StringComparison.InvariantCultureIgnoreCase);
        AddValue("INVARIANT_CULTURE_IGNORE_CASE", null, StringComparison.InvariantCultureIgnoreCase, deprecation);
        AddValue("ordinal", null, StringComparison.Ordinal);
        AddValue("ORDINAL", null, StringComparison.Ordinal, deprecation);
        AddValue("ordinalIgnoreCase", null, StringComparison.OrdinalIgnoreCase);
        AddValue("ORDINAL_IGNORE_CASE", null, StringComparison.OrdinalIgnoreCase, deprecation);
    }
}