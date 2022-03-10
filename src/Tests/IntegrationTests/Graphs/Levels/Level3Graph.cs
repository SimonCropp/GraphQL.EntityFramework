using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("Level3")]
public class Level3Graph :
    EfObjectGraphType<IntegrationDbContext, Level3Entity>
{
    public Level3Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}