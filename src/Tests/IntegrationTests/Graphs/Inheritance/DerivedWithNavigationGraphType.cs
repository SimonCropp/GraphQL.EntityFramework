public class DerivedWithNavigationGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase,
            resolve: _ => _.Projection);
        AddNavigationConnectionField(
            name: "childrenFromDerived",
            projection: _ => _.Children,
            resolve: _ => _.Projection);
        AutoMap();
        Interface<BaseGraphType>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}
