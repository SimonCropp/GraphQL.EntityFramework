using GraphQL.EntityFramework;
using GraphQL.Types.Relay;

public class DerivedGraph :
    EfObjectGraphType<IntegrationDbContext, DerivedEntity>
{
    public DerivedGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            e => e.Source.ChildrenFromBase);
        AutoMap();
        Interface<InterfaceGraph>();
        IsTypeOf = obj => obj is DerivedEntity;
    }
}