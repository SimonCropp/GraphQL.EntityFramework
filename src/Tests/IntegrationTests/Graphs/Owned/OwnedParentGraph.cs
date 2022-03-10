using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(OwnedParent))]
public class OwnedParentGraph :
    EfObjectGraphType<IntegrationDbContext, OwnedParent>
{
    public OwnedParentGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}