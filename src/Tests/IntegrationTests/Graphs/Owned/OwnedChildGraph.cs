using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(OwnedChild))]
public class OwnedChildGraph :
    EfObjectGraphType<IntegrationDbContext, OwnedChild>
{
    public OwnedChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}