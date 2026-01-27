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
        AutoMap();
    }
}
