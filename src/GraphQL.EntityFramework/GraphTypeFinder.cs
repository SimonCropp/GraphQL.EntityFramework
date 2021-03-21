using System;
using System.Linq;
using GraphQL.Types;

static class GraphTypeFinder
{
    public static Type FindGraphType<TReturn>(ISchema schema)
        where TReturn : class
    {
        var graphType = schema.TypeMappings
            .Where(x=>x.graphType == typeof(TReturn))
            .Select(x=>x.clrType)
            .SingleOrDefault();
        if (graphType != null)
        {
            return graphType;
        }

        throw new($"Could not resolve a GraphType for {typeof(TReturn).FullName}. Either pass in a GraphType explicitly or register a GraphType using GraphTypeTypeRegistry.Register<{typeof(TReturn).FullName},MyGraphType>().");
    }
}