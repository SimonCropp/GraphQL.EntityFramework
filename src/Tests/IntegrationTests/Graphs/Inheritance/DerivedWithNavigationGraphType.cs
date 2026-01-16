public class DerivedWithNavigationGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            _ => _.Source.ChildrenFromBase);
        AddNavigationConnectionField(
            name: "childrenFromDerived",
            projection: _ => _.Children,
            resolve: ctx => ctx.Projection);
        AutoMap();
        Interface<BaseGraphType>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}