﻿using GraphQL;
using GraphQL.EntityFramework;
using Xunit;

[GraphQLMetadata("WithManyChildren")]
public class WithManyChildrenGraph :
    EfObjectGraphType<IntegrationDbContext, WithManyChildrenEntity>
{
    public WithManyChildrenGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "child1",
            resolve: context =>
            {
                Assert.NotNull(context.Source!.Child2);
                Assert.NotNull(context.Source.Child1);
                return context.Source.Child1;
            },
            includeNames: new []{ "Child2", "Child1" });
        AutoMap();
    }
}