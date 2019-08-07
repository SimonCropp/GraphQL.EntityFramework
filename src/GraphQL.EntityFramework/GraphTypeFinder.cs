using System;
using GraphQL.Utilities;

static class GraphTypeFinder
{
    public static Type FindGraphType<TReturn>()
        where TReturn : class
    {
        var graphType = GraphTypeTypeRegistry.Get<TReturn>();
        if (graphType != null)
        {
            return graphType;
        }
        throw new Exception($"Could not resolve a GraphType for {typeof(TReturn).FullName}. Either pass in a GraphType explicitly or register a GraphType using GraphTypeTypeRegistry.Register<{typeof(TReturn).FullName},MyGraphType>().");
    }
}