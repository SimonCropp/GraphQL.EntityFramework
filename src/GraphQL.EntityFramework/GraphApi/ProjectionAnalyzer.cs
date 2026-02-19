static class ProjectionAnalyzer
{
    public static IReadOnlySet<string> ExtractPropertyPaths<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection) =>
        ExtractPropertyPaths((LambdaExpression)projection);

    public static IReadOnlySet<string> ExtractPropertyPaths(LambdaExpression projection)
    {
        var visitor = new PropertyAccessVisitor(projection.Parameters);
        visitor.Visit(projection);
        return visitor.AccessedProperties;
    }

    sealed class PropertyAccessVisitor(IReadOnlyCollection<ParameterExpression> parameters) : ExpressionVisitor
    {
        public HashSet<string> AccessedProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override Expression VisitMember(MemberExpression node)
        {
            var path = new List<string>();
            Expression? current = node;

            while (current is MemberExpression or UnaryExpression)
            {
                if (current is MemberExpression memberExpr)
                {
                    path.Insert(0, memberExpr.Member.Name);
                    current = memberExpr.Expression;
                }
                else if (current is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } unary)
                {
                    current = unary.Operand;
                }
                else
                {
                    break;
                }
            }

            if (current is ParameterExpression param &&
                parameters.Contains(param) &&
                path.Count > 0)
            {
                AccessedProperties.Add(string.Join('.', path));
            }

            return base.VisitMember(node);
        }
    }
}
