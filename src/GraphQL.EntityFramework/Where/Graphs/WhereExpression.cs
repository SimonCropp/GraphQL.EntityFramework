namespace GraphQL.EntityFramework;

public class WhereExpression
{
    public string Path { get; set; } = string.Empty;
    public Comparison Comparison { get; set; } = Comparison.Equal;
    public string[]? Value { get; set; }
    public bool Negate { get; set; }
    public Connector Connector { get; set; } = Connector.And;
    public WhereExpression[]? GroupedExpressions { get; set; }
}