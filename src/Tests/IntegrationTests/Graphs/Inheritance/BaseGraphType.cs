public class BaseGraphType :
    EfInterfaceGraphType<IntegrationDbContext, BaseEntity>
{
    public BaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AddNavigationConnectionField<DerivedChildEntity>(
            name: "childrenFromInterface",
            includeNames: ["ChildrenFromBase"]);
}