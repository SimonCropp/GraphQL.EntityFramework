public class DiscriminatorDerivedBGraphType :
    EfObjectGraphType<IntegrationDbContext, DiscriminatorDerivedBEntity>
{
    public DiscriminatorDerivedBGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
