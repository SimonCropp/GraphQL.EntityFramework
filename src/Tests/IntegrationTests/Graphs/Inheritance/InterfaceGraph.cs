using GraphQL.EntityFramework;
using GraphQL.Types.Relay;

public class InterfaceGraph :
    EfInterfaceGraphType<IntegrationDbContext, InheritedEntity>
{
    public InterfaceGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(e => e.Id);
        Field(e => e.Property, nullable: true);
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            includeNames: new[] { "ChildrenFromBase" });
    }
}