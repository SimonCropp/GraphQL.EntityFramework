using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("NamedId")]
public class NamedIdGraph :
    EfObjectGraphType<IntegrationDbContext, NamedIdEntity>
{
    public NamedIdGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}