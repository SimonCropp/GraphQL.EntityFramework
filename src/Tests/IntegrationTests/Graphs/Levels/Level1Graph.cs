using GraphQL.EntityFramework;

public class Level1Graph :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public Level1Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}