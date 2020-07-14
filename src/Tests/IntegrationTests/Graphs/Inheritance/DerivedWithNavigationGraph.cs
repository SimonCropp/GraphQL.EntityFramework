using GraphQL.EntityFramework;

public class DerivedWithNavigationGraph :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
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
        Interface<InterfaceGraph>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}