namespace GraphQL.EntityFramework
{
    public enum Comparison
    {
        // Both
        Equal,
        In,

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
}