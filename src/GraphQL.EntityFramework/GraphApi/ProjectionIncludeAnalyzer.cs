using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GraphQL.EntityFramework;

static class ProjectionIncludeAnalyzer
{
    public static IReadOnlySet<string> ExtractNavigationIncludes<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        IModel model)
        where TEntity : class
    {
        var visitor = new NavigationPropertyVisitor(model, typeof(TEntity));
        visitor.Visit(projection);
        return visitor.NavigationIncludes;
    }

    sealed class NavigationPropertyVisitor(IModel model, Type entityType) : ExpressionVisitor
    {
        public HashSet<string> NavigationIncludes { get; } = [];

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
            if (current is ParameterExpression param &&
                param.Type == entityType &&
                path.Count > 0)
            {
                // Extract navigation property paths (exclude scalar properties)
                ExtractNavigationPaths(path);
            }

            return base.VisitMember(node);
        }

        void ExtractNavigationPaths(List<string> path)
        {
            // Walk through the path and identify navigation properties
            var currentType = entityType;
            var currentPath = new List<string>();

            for (var i = 0; i < path.Count; i++)
            {
                var propertyName = path[i];

                if (currentType == null)
                {
                    break;
                }

                var entityTypeInfo = model.FindEntityType(currentType);
                if (entityTypeInfo == null)
                {
                    break;
                }

                var navigation = entityTypeInfo.FindNavigation(propertyName);
                if (navigation != null)
                {
                    // This is a navigation property - add it to includes
                    currentPath.Add(propertyName);
                    var includePath = string.Join('.', currentPath);
                    NavigationIncludes.Add(includePath);

                    // Move to the target type for the next iteration
                    currentType = navigation.TargetEntityType.ClrType;
                }
                else
                {
                    // This is a scalar property or not found - stop here
                    break;
                }
            }
        }
    }
}
