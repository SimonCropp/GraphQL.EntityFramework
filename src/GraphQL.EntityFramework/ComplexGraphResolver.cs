static class ComplexGraphResolver
{
    class Resolved(Type? entityType, IComplexGraphType? graph)
    {
        public readonly IComplexGraphType? Graph = graph;
        public readonly Type? EntityType = entityType;
    }

    static ConcurrentDictionary<IGraphType, Resolved> cache = [];

    public static bool TryGetComplexGraph(this FieldType fieldType, [NotNullWhen(true)] out IComplexGraphType? graph)
    {
        var orAdd = GetOrAdd(fieldType);
        graph = orAdd.Graph;
        return graph != null;
    }

    public static bool TryGetEntityTypeForField(this FieldType fieldType, [NotNullWhen(true)] out Type? entityType)
    {
        var orAdd = GetOrAdd(fieldType);
        entityType = orAdd.EntityType;
        return entityType != null;
    }

    static Resolved GetOrAdd(FieldType fieldType) =>
        cache.GetOrAdd(
            fieldType.ResolvedType!,
            graphType =>
            {
                if (graphType is ListGraphType listGraphType)
                {
                    graphType = listGraphType.ResolvedType!;
                }

                if (graphType is UnionGraphType unionGraphType)
                {
                    graphType = unionGraphType.PossibleTypes.First();
                }

                if (graphType is NonNullGraphType nonNullGraphType)
                {
                    graphType = nonNullGraphType.ResolvedType!;
                    if (graphType is ListGraphType innerListGraphType)
                    {
                        graphType = innerListGraphType.ResolvedType!;
                        if (graphType is NonNullGraphType innerNonNullGraphType)
                        {
                            graphType = innerNonNullGraphType.ResolvedType!;
                        }

                        if (graphType is UnionGraphType innerUnionGraphType)
                        {
                            graphType = innerUnionGraphType.PossibleTypes.First();
                        }
                    }
                }

                IComplexGraphType? graph = null;
                if (graphType is IComplexGraphType complexType)
                {
                    graph = complexType;
                }

                return new(ResolvedEntityType(graphType), graph);
            });

    static Type? ResolvedEntityType(IGraphType graph)
    {
        var type = graph.GetType();

        while (type is not null)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                if (genericTypeDefinition == typeof(ComplexGraphType<>))
                {
                    return genericArguments.Single();
                }
                if (genericTypeDefinition == typeof(ConnectionType<>))
                {
                    var resolvedEntityType = genericArguments.Single();
                    type = resolvedEntityType.BaseType;
                    continue;
                }
                if (genericTypeDefinition == typeof(ConnectionType<,>))
                {
                    var resolvedEntityType = genericArguments.First();
                    type = resolvedEntityType.BaseType;
                    continue;
                }
            }
            type = type.BaseType;
        }

        return null;
    }

    internal static IComplexGraphType GetComplexGraph(this FieldType fieldType)
    {
        if (TryGetComplexGraph(fieldType, out var complex))
        {
            return complex;
        }

        throw new($"Could not find resolve a {nameof(IComplexGraphType)} for {fieldType.GetType().FullName}.");
    }
}