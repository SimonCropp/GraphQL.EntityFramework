using GraphQL.EntityFramework;

public class Level2GraphType :
    EfObjectGraphType<IntegrationDbContext, Level2Entity>
{
    public Level2GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}