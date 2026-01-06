using System.Linq.Expressions;

namespace GraphQL.EntityFramework;

static class FilterProjectionAnalyzer
{
    public static IReadOnlySet<string> ExtractRequiredProperties<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection)
    {
        var visitor = new PropertyAccessVisitor(typeof(TEntity));
        visitor.Visit(projection);
        return visitor.AccessedProperties;
    }

    sealed class PropertyAccessVisitor(Type entityType) : ExpressionVisitor
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

            // Check if the root is a parameter of the entity type
            if (current is ParameterExpression param && param.Type == entityType && path.Count > 0)
            {
                // Add the full path (e.g., "Parent.Name" or just "Name")
                AccessedProperties.Add(string.Join(".", path));
            }

            return base.VisitMember(node);
        }
    }
}
