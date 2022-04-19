using GraphQL.EntityFramework;

public class OwnedParentGraphType :
    EfObjectGraphType<IntegrationDbContext, OwnedParent>
{
    public OwnedParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}