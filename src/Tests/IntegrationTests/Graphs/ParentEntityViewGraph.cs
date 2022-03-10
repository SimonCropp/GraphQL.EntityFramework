using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("ParentView")]
public class ParentEntityViewGraph :
    EfObjectGraphType<IntegrationDbContext, ParentEntityView>
{
    public ParentEntityViewGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}