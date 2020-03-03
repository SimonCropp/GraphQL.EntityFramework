namespace GraphQL.EntityFramework
{
    public enum Comparison
    {
        // Both
        Equal,
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
}