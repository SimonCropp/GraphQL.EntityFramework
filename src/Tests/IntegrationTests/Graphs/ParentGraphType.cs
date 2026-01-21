public class ParentGraphType :
    EfObjectGraphType<IntegrationDbContext, ParentEntity>
{
    public ParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            projection: _ => _.Children,
            resolve: _ => _.Projection);
        AddNavigationConnectionField(
            name: "childrenConnectionOmitQueryArguments",
            projection: _ => _.Children,
            resolve: _ => _.Projection,
            omitQueryArguments: true);
        AutoMap();
    }
}
