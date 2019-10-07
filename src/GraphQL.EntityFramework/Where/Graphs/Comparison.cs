namespace GraphQL.EntityFramework
{
    public enum Comparison
    {
        // Both
        Equal,
        In,

        // Object
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
}