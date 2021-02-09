using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("ManyToManyRight")]
public class ManyToManyRightGraph :
    EfObjectGraphType<IntegrationDbContext, ManyToManyRightEntity>
{
    public ManyToManyRightGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}