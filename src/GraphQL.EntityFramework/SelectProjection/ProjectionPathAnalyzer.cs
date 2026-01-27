namespace GraphQL.EntityFramework;

/// <summary>
/// Analyzes projection expressions to extract property paths accessed from the source type.
/// Used by IncludeAppender to determine which navigations to include in the Select projection.
/// </summary>
static class ProjectionPathAnalyzer
{
    public static IReadOnlySet<string> ExtractPropertyPaths(LambdaExpression projection, Type sourceType)
    {
        // Get all parameters from the lambda
        var parameters = new HashSet<ParameterExpression>(projection.Parameters);

        var visitor = new PropertyAccessVisitor(parameters);
        visitor.Visit(projection);
        return visitor.AccessedProperties;
    }

    sealed class PropertyAccessVisitor(HashSet<ParameterExpression> parameters) : ExpressionVisitor
    {
        public HashSet<string> AccessedProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override Expression VisitMember(MemberExpression node)
        {
            // Build the full property path by walking up the expression tree
            var path = new List<string>();
            Expression? current = node;

            while (current is MemberExpression memberExpr)
            {
                path.Insert(0, memberExpr.Member.Name);
                current = memberExpr.Expression;
            }

            // Check if the root is a parameter expression from the lambda
            if (current is ParameterExpression param &&
                parameters.Contains(param) &&
                path.Count > 0)
            {
                // Add the full path (e.g., "Parent.Name" or just "Name")
                AccessedProperties.Add(string.Join('.', path));
            }

            return base.VisitMember(node);
        }
    }
}
