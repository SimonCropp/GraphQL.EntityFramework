using System;

namespace GraphQL.EntityFramework
{
    public class WhereExpression
    {
        public string Path { get; set; } = null!;
        public Comparison? Comparison { get; set; }
        public StringComparison? Case { get; set; }
        public string[]? Value { get; set; }
    }
}