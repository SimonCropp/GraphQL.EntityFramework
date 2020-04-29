using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GraphQL.Types;
using GraphQL.Types.Relay;

static class ComplexGraphResolver
{
    class Resolved
    {
        public Resolved(Type? entityType, IComplexGraphType? graph)
        {
            EntityType = entityType;
            Graph = graph;
        }
        public readonly IComplexGraphType? Graph;
        public readonly Type? EntityType;
    }

    static ConcurrentDictionary<IGraphType, Resolved> cache = new ConcurrentDictionary<IGraphType, Resolved>();

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

    static Resolved GetOrAdd(FieldType fieldType)
    {
        return cache.GetOrAdd(
            fieldType.ResolvedType,
            graphType =>
            {
                if (graphType is ListGraphType listGraphType)
                {
                    graphType = listGraphType.ResolvedType;
                }

                if (graphType is NonNullGraphType nonNullGraphType)
                {
                    graphType = nonNullGraphType.ResolvedType;
                }

                IComplexGraphType? graph = null;
                if (graphType is IComplexGraphType complexType)
                {
                    graph = complexType;
                }

                return new Resolved(ResolvedEntityType(graphType), graph);
            });
    }

    static Type? ResolvedEntityType(IGraphType graph)
    {
        var type = graph.GetType();

        while (type != null)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(ComplexGraphType<>))
                {
                    return type.GetGenericArguments().Single();
                }
                if (genericTypeDefinition == typeof(ConnectionType<>))
                {
                    var resolvedEntityType = type.GetGenericArguments().Single();
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

        throw new Exception($"Could not find resolve a {nameof(IComplexGraphType)} for {fieldType.GetType().FullName}.");
    }
}