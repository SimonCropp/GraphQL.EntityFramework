namespace GraphQL.EntityFramework;

public class WhereExpression
{
    public string Path { get; set; } = string.Empty;
    public Comparison Comparison { get; set; } = Comparison.Equal;

    /// <summary>
    /// The values compared against. Typed when they come from the where argument; strings are
    /// converted to the property type.
    /// </summary>
    public object?[]? Value { get; set; }

    public bool Negate { get; set; }
    public Connector Connector { get; set; } = Connector.And;
    public WhereExpression[]? GroupedExpressions { get; set; }

    /// <summary>
    /// When set, <see cref="Path"/> is a collection and <see cref="GroupedExpressions"/> is the
    /// predicate on its items.
    /// </summary>
    public Quantifier? Quantifier { get; set; }

    internal bool IsEmpty =>
        Path.Length == 0 &&
        Quantifier is null &&
        GroupedExpressions is not { Length: > 0 };
}
