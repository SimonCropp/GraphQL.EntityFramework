using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraphType :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}