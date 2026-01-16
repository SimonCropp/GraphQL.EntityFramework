public class DerivedWithNavigationGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase,
            resolve: ctx => ctx.Projection);
        AddNavigationConnectionField(
            name: "childrenFromDerived",
            projection: _ => _.Children,
            resolve: ctx => ctx.Projection);
        AutoMap();
        Interface<BaseGraphType>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}