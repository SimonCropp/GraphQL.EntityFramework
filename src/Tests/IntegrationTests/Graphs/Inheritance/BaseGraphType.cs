public class BaseGraphType :
    EfInterfaceGraphType<IntegrationDbContext, BaseEntity>
{
    public BaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase);
}