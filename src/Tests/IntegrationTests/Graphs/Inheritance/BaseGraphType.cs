public class BaseGraphType :
    EfInterfaceGraphType<IntegrationDbContext, BaseEntity>
{
    public BaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(_ => _.Id);
        Field(_ => _.Property, nullable: true);
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            includeNames: ["ChildrenFromBase"]);
    }
}