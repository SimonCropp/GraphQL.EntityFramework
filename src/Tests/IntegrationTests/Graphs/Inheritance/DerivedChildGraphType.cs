public class DerivedChildGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedChildEntity>
{
    public DerivedChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        // Add navigation to abstract parent - this is what triggers the bug
        AddNavigationField(
            name: "parent",
            projection: _ => _.Parent,
            resolve: _ => _.Projection,
            graphType: typeof(BaseGraphType));
        AutoMap(["Parent", "TypedParent"]);
    }
}
