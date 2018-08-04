using System;
using System.Collections.Concurrent;
using System.Linq;
using GraphQL.Types;

static class ComplexGraphResolver
{
    class Resolved
    {
        public IComplexGraphType ComplexGraphType;
        public Type EntityType;
    }

    static ConcurrentDictionary<IGraphType, Resolved> cache = new ConcurrentDictionary<IGraphType, Resolved>();

    public static bool TryGetComplexGraph(this FieldType fieldType, out IComplexGraphType complexGraph)
    {
        var orAdd = GetOrAdd(fieldType);
        complexGraph = orAdd.ComplexGraphType;
        return complexGraph != null;
    }

    public static bool TryGetEntityTypeForField(this FieldType fieldType, out Type entityType)
    {
        var orAdd = GetOrAdd(fieldType);
        entityType = orAdd.EntityType;
        return entityType != null;
    }

    static Resolved GetOrAdd(FieldType fieldType)
    {
        var orAdd = cache.GetOrAdd(fieldType.ResolvedType, graphType =>
        {
            var resolved = new Resolved();
            if (graphType is ListGraphType listGraphType)
            {
                graphType = listGraphType.ResolvedType;
            }

            if (graphType is IComplexGraphType complexType)
            {
                resolved.ComplexGraphType = complexType;
            }

            resolved.EntityType = ResolvedEntityType(graphType);
            return resolved;
        });
        return orAdd;
    }

    static Type ResolvedEntityType(IGraphType graphType)
    {
        var type = graphType.GetType();

        while (type.BaseType != null)
        {
            type = type.BaseType;
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(ComplexGraphType<>))
            {
                return type.GetGenericArguments().Single();
            }
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