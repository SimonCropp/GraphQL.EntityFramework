using GraphQL.EntityFramework;

public class ParentEntityViewGraphType :
    EfObjectGraphType<IntegrationDbContext, ParentEntityView>
{
    public ParentEntityViewGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}