using GraphQL.EntityFramework;

public class Child2Graph :
    EfObjectGraphType<IntegrationDbContext, Child2Entity>
{
    public Child2Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}