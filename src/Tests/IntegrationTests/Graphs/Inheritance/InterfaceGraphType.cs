public class InterfaceGraphType :
    EfInterfaceGraphType<IntegrationDbContext, InheritedEntity>
{
    public InterfaceGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(_ => _.Id);
        Field(_ => _.Property, nullable: true);
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            includeNames: [ "ChildrenFromBase" ]);
    }
}