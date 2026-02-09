public class DiscriminatorDerivedAGraphType :
    EfObjectGraphType<IntegrationDbContext, DiscriminatorDerivedAEntity>
{
    public DiscriminatorDerivedAGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
