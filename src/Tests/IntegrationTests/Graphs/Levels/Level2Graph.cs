using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("Level2")]
public class Level2Graph :
    EfObjectGraphType<IntegrationDbContext, Level2Entity>
{
    public Level2Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}