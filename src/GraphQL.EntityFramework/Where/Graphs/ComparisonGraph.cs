using GraphQL.EntityFramework;
using GraphQL.Types;

class ComparisonGraph :
    EnumerationGraphType
{
    public ComparisonGraph()
    {
        Name = nameof(Comparison);
        Add("contains", Comparison.Contains);
        Add("endsWith", Comparison.EndsWith);
        Add("equal", Comparison.Equal);
        Add("greaterThan", Comparison.GreaterThan);
        Add("greaterThanOrEqual", Comparison.GreaterThanOrEqual);
        Add("notIn", Comparison.NotIn, "Negation Property used with the 'in' comparison should be used in place of this");
        Add("in", Comparison.In);
        Add("lessThan", Comparison.LessThan);
        Add("lessThanOrEqual", Comparison.LessThanOrEqual);
        Add("like", Comparison.Like);
        Add("startsWith", Comparison.StartsWith);
    }

    //TODO:
    // public override object? ParseLiteral(IValue value)
    // {
    //     var literal = base.ParseLiteral(value);
    //     if (literal is not null)
    //     {
    //         return literal;
    //     }
    //
    //     if (value is StringValue str)
    //     {
    //         var strValue = str.Value;
    //         if (Enum.TryParse(strValue, true, out Comparison comparison))
    //         {
    //             return comparison;
    //         }
    //     }
    //
    //     return null;
    // }
}