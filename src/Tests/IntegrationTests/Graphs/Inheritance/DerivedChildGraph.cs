﻿using GraphQL.EntityFramework;
using GraphQL;

[GraphQLMetadata("DerivedChild")]
public class DerivedChildGraph :
    EfObjectGraphType<IntegrationDbContext, DerivedChildEntity>
{
    public DerivedChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap(new List<string> { "Parent", "TypedParent" });
    }
}