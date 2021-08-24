using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("Derived")]
public class DerivedGraph :
    EfObjectGraphType<IntegrationDbContext, DerivedEntity>
{
    public DerivedGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            e => e.Source!.ChildrenFromBase);
        AutoMap();
        Interface<InterfaceGraph>();
        IsTypeOf = obj => obj is DerivedEntity;
    }
}