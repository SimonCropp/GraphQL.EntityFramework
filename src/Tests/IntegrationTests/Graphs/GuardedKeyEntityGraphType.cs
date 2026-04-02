public class GuardedKeyEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, GuardedKeyEntity>
{
    public GuardedKeyEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
