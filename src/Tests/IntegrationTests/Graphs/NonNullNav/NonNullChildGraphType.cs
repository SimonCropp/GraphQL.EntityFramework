public class NonNullChildGraphType :
    EfObjectGraphType<IntegrationDbContext, NonNullChildEntity>
{
    public NonNullChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}