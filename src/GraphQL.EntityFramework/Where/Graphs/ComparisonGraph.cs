using System;
using GraphQL.EntityFramework;
using GraphQL.Language.AST;
using GraphQL.Types;

class ComparisonGraph :
    EnumerationGraphType
{
    public ComparisonGraph()
    {
        AddValue("contains", null, Comparison.Contains);
        AddValue("endsWith", null, Comparison.EndsWith);
        AddValue("equal", null, Comparison.Equal);
        AddValue("greaterThan", null, Comparison.GreaterThan);
        AddValue("greaterThanOrEqual", null, Comparison.GreaterThanOrEqual);
        AddValue("in", null, Comparison.In);
        AddValue("notIn", null, Comparison.NotIn);
        AddValue("lessThan", null, Comparison.LessThan);
        AddValue("lessThanOrEqual", null, Comparison.LessThanOrEqual);
        AddValue("like", null, Comparison.Like);
        AddValue("notEqual", null, Comparison.NotEqual);
        AddValue("startsWith", null, Comparison.StartsWith);
    }

    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(value);
        if (literal != null)
        {
            return literal;
        }

        if (value is StringValue str)
        {
            var strValue = str.Value;
            if (Enum.TryParse(strValue, true, out Comparison comparison))
            {
                return comparison;
            }
        }

        return null;
    }
}