using GraphQL.EntityFramework;

public class ManyToManyLeftGraph :
    EfObjectGraphType<IntegrationDbContext, ManyToManyLeftEntity>
{
    public ManyToManyLeftGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}