public class TphDerivedNavOwnerGraphType :
    EfObjectGraphType<IntegrationDbContext, TphDerivedNavOwnerEntity>
{
    public TphDerivedNavOwnerGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
