static class ProjectionAnalyzer
{
    public static IReadOnlySet<string> ExtractRequiredProperties<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection)
    {
        var (properties, _) = AnalyzeProjection<TEntity, TProjection>(projection);
        return properties;
    }

    /// <summary>
    /// Analyzes a projection expression in a single pass to extract:
    /// 1. Required property paths (for Include/Select building)
    /// 2. Abstract navigation accesses (for validation)
    /// </summary>
    public static (IReadOnlySet<string> Properties, IReadOnlyList<AbstractNavigationAccess> AbstractAccesses)
        AnalyzeProjection<TEntity, TProjection>(Expression<Func<TEntity, TProjection>> projection)
    {
        var visitor = new ProjectionVisitor(typeof(TEntity));
        visitor.Visit(projection);
        return (visitor.AccessedProperties, visitor.AbstractNavigationAccesses);
    }

    public record AbstractNavigationAccess(string NavigationName, Type AbstractType);

    sealed class ProjectionVisitor(Type entityType) : ExpressionVisitor
    {
        public HashSet<string> AccessedProperties { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<AbstractNavigationAccess> AbstractNavigationAccesses { get; } = [];

        protected override Expression VisitMember(MemberExpression node)
        {
            // Build the full property path by walking up the expression tree
            var path = new List<string>();
            var propertyTypes = new List<Type>();
            Expression? current = node;

            while (current is MemberExpression memberExpr)
            {
                path.Insert(0, memberExpr.Member.Name);
                if (memberExpr.Member is PropertyInfo prop)
                {
                    propertyTypes.Insert(0, prop.PropertyType);
                }
                current = memberExpr.Expression;
            }

            // Check if the root is a parameter of the entity type
            if (current is ParameterExpression param &&
                param.Type == entityType &&
                path.Count > 0)
            {
                // Add the full path (e.g., "Parent.Name" or just "Name")
                AccessedProperties.Add(string.Join('.', path));

                // Check for abstract navigation access (path.Count > 1 means navigation.Property)
                if (path.Count > 1 && propertyTypes.Count > 0)
                {
                    var navType = GetElementType(propertyTypes[0]);
                    if (navType.IsAbstract)
                    {
                        AbstractNavigationAccesses.Add(new(path[0], navType));
                    }
                }
            }

            return base.VisitMember(node);
        }

        static Type GetElementType(Type type)
        {
            // For collections, get the element type
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(ICollection<>) ||
                    genericDef == typeof(IList<>) ||
                    genericDef == typeof(IEnumerable<>) ||
                    genericDef == typeof(List<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return type;
        }
    }
}
