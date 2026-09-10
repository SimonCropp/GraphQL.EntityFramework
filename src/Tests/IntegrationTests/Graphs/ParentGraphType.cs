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
        Field<ListGraphType<ChildGraphType>, IEnumerable<ChildEntity>>("childrenViaResolveList")
            .ResolveList(GraphQlService, projection: _ => _.Children, resolve: _ => _.Projection);
        Field<ListGraphType<ChildGraphType>, IEnumerable<ChildEntity>>("childrenViaResolveListAsync")
            .ResolveListAsync(GraphQlService, projection: _ => _.Children, resolve: _ => Task.FromResult<IEnumerable<ChildEntity>>(_.Projection));
        // The resolver-less overloads on an object type
        AddNavigationListField(
            name: "childrenNoResolve",
            projection: _ => _.Children);
        AddNavigationConnectionField(
            name: "childrenConnectionNoResolve",
            projection: _ => _.Children);
        AutoMap();
    }
}
