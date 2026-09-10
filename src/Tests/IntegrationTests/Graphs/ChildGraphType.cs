public class ChildGraphType :
    EfObjectGraphType<IntegrationDbContext, ChildEntity>
{
    public ChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parentAlias",
            projection: _ => _.Parent,
            resolve: _ => _.Projection,
            graphType: typeof(ParentGraphType));
        // Field<TGraphType> builds an object typed builder, so the filter applied has to be
        // chosen by the runtime type of the result
        Field<ParentGraphType>("parentObject")
            .Resolve(
                projection: _ => _.Parent,
                resolve: _ => _.Projection!);
        AddNavigationField(
            name: "parentNoResolve",
            projection: _ => _.Parent,
            graphType: typeof(ParentGraphType));
        AutoMap();
    }
}
