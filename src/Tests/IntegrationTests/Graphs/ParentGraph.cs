﻿using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("Parent")]
public class ParentGraph :
    EfObjectGraphType<IntegrationDbContext, ParentEntity>
{
    public ParentGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] { "Children" });
        AutoMap();
    }
}