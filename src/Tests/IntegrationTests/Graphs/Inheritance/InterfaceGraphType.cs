using GraphQL.EntityFramework;

public class InterfaceGraphType :
    EfInterfaceGraphType<IntegrationDbContext, InheritedEntity>
{
    public InterfaceGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(e => e.Id);
        Field(e => e.Property, nullable: true);
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            includeNames: new[] { "ChildrenFromBase" });
    }
}