using GraphQL.EntityFramework;

public class DerivedWithNavigationGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            e => e.Source.ChildrenFromBase);
        AddNavigationConnectionField(
            name: "childrenFromDerived",
            e => e.Source.Children,
            includeNames: new[] { "Children" });
        AutoMap();
        Interface<InterfaceGraphType>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}