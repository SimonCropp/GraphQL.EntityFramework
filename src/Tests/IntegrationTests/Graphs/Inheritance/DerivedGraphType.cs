public class DerivedGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedEntity>
{
    public DerivedGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase,
            resolve: _ => _.Projection);
        AutoMap();
        Interface<BaseGraphType>();
        IsTypeOf = obj => obj is DerivedEntity;
    }
}
