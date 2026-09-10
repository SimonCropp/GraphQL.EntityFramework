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
        AddNavigationListField(
            name: "childrenOmitQueryArguments",
            projection: _ => _.Children,
            resolve: _ => _.Projection,
            omitQueryArguments: true);
        // Returns a collection other than the projected one, so its arguments stay in memory
        AddNavigationListField(
            name: "childrenReversed",
            projection: _ => _.Children,
            resolve: _ => _.Projection.Reverse());
        AutoMap();
    }
}
