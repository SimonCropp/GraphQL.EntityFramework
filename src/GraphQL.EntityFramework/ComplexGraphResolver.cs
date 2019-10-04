using System;
using System.Collections.Concurrent;
using System.Linq;
using GraphQL.Types;
using GraphQL.Types.Relay;

static class ComplexGraphResolver
{
    class Resolved
    {
        public Resolved(Type? entityType, IComplexGraphType? complexGraphType)
        {
            EntityType = entityType;
            ComplexGraphType = complexGraphType;
        }
        public readonly IComplexGraphType? ComplexGraphType;
        public readonly Type? EntityType;
    }

    static ConcurrentDictionary<IGraphType, Resolved> cache = new ConcurrentDictionary<IGraphType, Resolved>();

    public static bool TryGetComplexGraph(this FieldType fieldType, out IComplexGraphType? complexGraph)
    {
        var orAdd = GetOrAdd(fieldType);
        complexGraph = orAdd.ComplexGraphType;
        return complexGraph != null;
    }

    public static bool TryGetEntityTypeForField(this FieldType fieldType, out Type? entityType)
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

                IComplexGraphType? complexGraphType = null;
                if (graphType is IComplexGraphType complexType)
                {
                    complexGraphType = complexType;
                }

                return new Resolved(ResolvedEntityType(graphType), complexGraphType);
            });
    }

    static Type? ResolvedEntityType(IGraphType graphType)
    {
        var type = graphType.GetType();

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
            return complex!;
        }

        throw new Exception($"Could not find resolve a {nameof(IComplexGraphType)} for {fieldType.GetType().FullName}.");
    }
}