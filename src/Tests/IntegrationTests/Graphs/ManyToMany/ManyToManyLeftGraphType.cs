using GraphQL.EntityFramework;

public class ManyToManyLeftGraphType :
    EfObjectGraphType<IntegrationDbContext, ManyToManyLeftEntity>
{
    public ManyToManyLeftGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}