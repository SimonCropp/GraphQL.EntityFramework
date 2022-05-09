using GraphQL.EntityFramework;
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
}