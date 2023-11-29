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
            _ => _.Source.Children,
            includeNames: [ "Children" ]);
        AutoMap();
        Interface<InterfaceGraphType>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}