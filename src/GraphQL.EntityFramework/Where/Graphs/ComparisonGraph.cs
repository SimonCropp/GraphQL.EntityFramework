using System;
using GraphQL.EntityFramework;
using GraphQL.Language.AST;
using GraphQL.Types;

class ComparisonGraph :
    EnumerationGraphType
{
    public ComparisonGraph()
    {
        Name = nameof(Comparison);
        AddValue("contains", null, Comparison.Contains);
        AddValue("endsWith", null, Comparison.EndsWith);
        AddValue("equal", null, Comparison.Equal);
        AddValue("greaterThan", null, Comparison.GreaterThan);
        AddValue("greaterThanOrEqual", null, Comparison.GreaterThanOrEqual);
        AddValue("notIn", null, Comparison.NotIn, "Negation Property used with the 'in' comparison should be used in place of this");
        AddValue("in", null, Comparison.In);
        AddValue("lessThan", null, Comparison.LessThan);
        AddValue("lessThanOrEqual", null, Comparison.LessThanOrEqual);
        AddValue("like", null, Comparison.Like);
        AddValue("startsWith", null, Comparison.StartsWith);
    }

    public override bool CanParseLiteral(IValue value)
    {
        value = TryToEnumValue(value);
        return base.CanParseLiteral(value);
    }

    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(TryToEnumValue(value));
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
    
    static IValue TryToEnumValue(IValue value)
    {
        if (value is StringValue stringValue)
        {
            return new EnumValue(stringValue.Value);
        }

        return value;
    }
}