using GraphQL.EntityFramework;

public class Level1GraphType :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public Level1GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}