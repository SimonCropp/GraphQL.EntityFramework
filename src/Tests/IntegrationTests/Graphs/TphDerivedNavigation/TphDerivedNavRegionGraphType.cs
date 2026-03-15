public class TphDerivedNavRegionGraphType :
    EfObjectGraphType<IntegrationDbContext, TphDerivedNavRegionEntity>
{
    public TphDerivedNavRegionGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
        Interface<TphDerivedNavBaseGraphType>();
        IsTypeOf = _ => _ is TphDerivedNavRegionEntity;
    }
}
