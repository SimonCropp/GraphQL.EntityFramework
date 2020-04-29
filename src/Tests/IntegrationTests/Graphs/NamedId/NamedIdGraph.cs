using GraphQL.EntityFramework;

public class NamedIdGraph :
    EfObjectGraphType<IntegrationDbContext, NamedIdEntity>
{
    public NamedIdGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}