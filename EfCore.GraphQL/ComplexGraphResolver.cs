using System;
using GraphQL.Types;

static class ComplexGraphResolver
{
    internal static bool TryGetComplexGraph(this FieldType fieldType, out IComplexGraphType complexGraph)
    {
        var graphType = fieldType.ResolvedType;
        if (graphType is ListGraphType)
        {
            graphType = ((ListGraphType) graphType).ResolvedType;
        }

        if (graphType is IComplexGraphType type)
        {
            complexGraph = type;
            return true;
        }

        complexGraph = null;
        return false;
    }

    internal static IComplexGraphType GetComplexGraph(this FieldType fieldType)
    {
        var graphType = fieldType.ResolvedType;
        if (graphType is ListGraphType)
        {
            graphType = ((ListGraphType) graphType).ResolvedType;
        }

        if (graphType is IComplexGraphType type)
        {
            return type;
        }

        throw new Exception($"Could not find resolve a {nameof(IComplexGraphType)} for {fieldType.GetType().FullName}.");
    }
}