﻿using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(MappingParent))]
public class MappingParentGraph :
    EfObjectGraphType<MappingContext, MappingParent>
{
    public MappingParentGraph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}