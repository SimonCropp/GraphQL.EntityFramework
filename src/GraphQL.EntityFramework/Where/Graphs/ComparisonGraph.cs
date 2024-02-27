class ComparisonGraph :
    EnumerationGraphType
{
    public ComparisonGraph()
    {
        Name = nameof(Comparison);
        Add("contains", Comparison.Contains);
        Add("endsWith", Comparison.EndsWith);
        Add("equal", Comparison.Equal);
        Add("notEqual", Comparison.NotEqual);
        Add("greaterThan", Comparison.GreaterThan);
        Add("greaterThanOrEqual", Comparison.GreaterThanOrEqual);
        Add("notIn", Comparison.NotIn, "Negation Property used with the 'in' comparison should be used in place of this");
        Add("in", Comparison.In);
        Add("lessThan", Comparison.LessThan);
        Add("lessThanOrEqual", Comparison.LessThanOrEqual);
        Add("like", Comparison.Like);
        Add("startsWith", Comparison.StartsWith);
    }
}