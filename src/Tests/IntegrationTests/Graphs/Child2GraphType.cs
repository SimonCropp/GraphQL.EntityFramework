using GraphQL.EntityFramework;

public class Child2GraphType :
    EfObjectGraphType<IntegrationDbContext, Child2Entity>
{
    public Child2GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}