using System;

namespace GraphQL.EntityFramework
{
    public class WhereExpression
    {
        public string Path { get; set; } = string.Empty;
        public Comparison Comparison { get; set; } = Comparison.Equal;
        public StringComparison? Case { get; set; } = null;
        public string[]? Value { get; set; }
        public bool Negate { get; set; } = false;
        public Connector Connector { get; set; } = Connector.And;
        public WhereExpression[]? GroupedExpressions { get; set; }
    }
}