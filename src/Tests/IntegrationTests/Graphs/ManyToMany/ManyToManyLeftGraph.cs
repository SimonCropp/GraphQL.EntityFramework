using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("ManyToManyLeft")]
public class ManyToManyLeftGraph :
    EfObjectGraphType<IntegrationDbContext, ManyToManyLeftEntity>
{
    public ManyToManyLeftGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}