public class ParentGraphType :
    EfObjectGraphType<IntegrationDbContext, ParentEntity>
{
    public ParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            projection: _ => _.Children,
            resolve: ctx => ctx.Projection);
        AddNavigationConnectionField(
            name: "childrenConnectionOmitQueryArguments",
            projection: _ => _.Children,
            resolve: ctx => ctx.Projection,
            omitQueryArguments: true);
        AutoMap();
    }
}