public class TphDerivedNavCategoryGraphType :
    EfObjectGraphType<IntegrationDbContext, TphDerivedNavCategoryEntity>
{
    public TphDerivedNavCategoryGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
        Interface<TphDerivedNavBaseGraphType>();
        IsTypeOf = _ => _ is TphDerivedNavCategoryEntity;
    }
}
