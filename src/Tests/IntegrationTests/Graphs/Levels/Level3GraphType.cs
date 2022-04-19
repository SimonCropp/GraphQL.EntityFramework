using GraphQL.EntityFramework;

public class Level3GraphType :
    EfObjectGraphType<IntegrationDbContext, Level3Entity>
{
    public Level3GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}