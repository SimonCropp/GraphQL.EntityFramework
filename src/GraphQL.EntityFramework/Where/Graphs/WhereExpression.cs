using System;
using System.Collections.Generic;

namespace GraphQL.EntityFramework
{
    public class WhereExpression
    {
        public string? Path { get; set; }
        public Comparison? Comparison { get; set; }
        public StringComparison? Case { get; set; }
        public string[] Value { get; set; }
        public Connector? Connector { get; set; }
        public WhereExpression[] GroupedExpressions { get; set;  }
    }
}