using GraphQL.EntityFramework;

public class ManyToManyRightGraph :
    EfObjectGraphType<IntegrationDbContext, ManyToManyRightEntity>
{
    public ManyToManyRightGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}