public class RegionGraphType :
    EfObjectGraphType<IntegrationDbContext, RegionEntity>
{
    public RegionGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
