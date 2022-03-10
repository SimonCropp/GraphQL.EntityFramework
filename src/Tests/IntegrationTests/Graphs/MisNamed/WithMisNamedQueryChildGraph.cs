using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("WithMisNamedQueryChild")]
public class WithMisNamedQueryChildGraph :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}