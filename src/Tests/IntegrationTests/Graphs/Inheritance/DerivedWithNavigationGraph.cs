using GraphQL.EntityFramework;
using GraphQL.Types.Relay;

public class DerivedWithNavigationGraph :
    EfObjectGraphType<IntegrationDbContext, DerivedWithNavigationEntity>
{
    public DerivedWithNavigationGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            e => e.Source.ChildrenFromBase);
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromDerived",
            e => e.Source.Children,
            includeNames: new[] { "Children" });
        AutoMap();
        Interface<InterfaceGraph>();
        IsTypeOf = obj => obj is DerivedWithNavigationEntity;
    }
}