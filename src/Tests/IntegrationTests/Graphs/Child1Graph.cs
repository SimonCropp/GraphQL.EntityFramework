using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("Child1")]
public class Child1Graph :
    EfObjectGraphType<IntegrationDbContext, Child1Entity>
{
    public Child1Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}