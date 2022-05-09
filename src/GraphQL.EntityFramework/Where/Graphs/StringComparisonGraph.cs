using GraphQL.Language.AST;
using GraphQL.Types;

class StringComparisonGraph :
    EnumerationGraphType<StringComparison>
{
    public StringComparisonGraph() =>
        Name = nameof(StringComparison);

    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(value);
        if (literal is not null)
        {
            return literal;
        }

        if (value is StringValue str)
        {
            if (Enum.TryParse(str.Value, true, out StringComparison comparison))
            {
                return comparison;
            }
        }

        return null;
    }
}