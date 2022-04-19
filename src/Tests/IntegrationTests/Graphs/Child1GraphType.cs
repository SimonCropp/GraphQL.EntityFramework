using GraphQL.EntityFramework;

public class Child1GraphType :
    EfObjectGraphType<IntegrationDbContext, Child1Entity>
{
    public Child1GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}