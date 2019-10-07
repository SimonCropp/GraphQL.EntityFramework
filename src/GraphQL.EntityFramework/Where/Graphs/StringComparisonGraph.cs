using System;
using GraphQL.Language.AST;
using GraphQL.Types;

class StringComparisonGraph :
    EnumerationGraphType<StringComparison>
{
    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(value);
        if (literal != null)
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