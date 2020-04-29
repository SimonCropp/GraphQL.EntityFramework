using System;
using GraphQL;
using GraphQL.EntityFramework;

public class MappingSchema :
    GraphQL.Types.Schema
{
    public MappingSchema(EfGraphQLService<MappingContext> graphQlService) :
        base(
            new FuncDependencyResolver(
                type =>
                {
                    if (type == typeof(MappingChildGraph))
                    {
                        return new MappingChildGraph(graphQlService);
                    }

                    if (type == typeof(MappingParentGraph))
                    {
                        return new MappingParentGraph(graphQlService);
                    }

                    return Activator.CreateInstance(type);
                }))
    {
        Query = new MappingQuery(graphQlService);
    }
}