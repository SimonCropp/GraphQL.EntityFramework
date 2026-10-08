public class BaseGraphType :
    EfInterfaceGraphType<IntegrationDbContext, BaseEntity>
{
    public BaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase);

        // An interface field has no resolver, so nothing to declare a projection for. The
        // implementing types declare what their resolvers read.
        AddField(
            new()
            {
                Name = "statusSummary",
                Type = typeof(StringGraphType)
            });
    }
}