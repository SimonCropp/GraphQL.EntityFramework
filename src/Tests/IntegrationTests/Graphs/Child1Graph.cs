using GraphQL.EntityFramework;

public class Child1Graph :
    EfObjectGraphType<IntegrationDbContext, Child1Entity>
{
    public Child1Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
    }
}