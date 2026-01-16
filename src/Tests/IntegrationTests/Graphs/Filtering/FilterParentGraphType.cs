public class FilterParentGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterParentEntity>
{
    public FilterParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            projection: _ => _.Children,
            resolve: ctx => ctx.Projection);
        AutoMap();
    }
}