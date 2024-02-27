namespace GraphQL.EntityFramework;

public enum Comparison
{
    // Both
    Equal,
    NotEqual,
    In,
    NotIn,

    // Object/ List
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // String
    StartsWith,
    EndsWith,
    Contains,
    Like
}