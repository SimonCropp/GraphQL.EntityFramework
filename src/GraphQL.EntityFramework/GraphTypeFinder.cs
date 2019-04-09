using System;
using GraphQL.Utilities;

static class GraphTypeFinder
{
    public static Type FindGraphType<TReturn>(Type graphType)
        where TReturn : class
    {
        if (graphType != null)
        {
            return graphType;
        }
        graphType = GraphTypeTypeRegistry.Get<TReturn>();
        if (graphType != null)
        {
            return graphType;
        }
        throw new Exception($"Could not resolve a GraphType for {typeof(TReturn).FullName}. Either pass in a GraphType explicitly or register a GraphType using GraphTypeTypeRegistry.Register<{typeof(TReturn).FullName},MyGraphType>().");
    }
}