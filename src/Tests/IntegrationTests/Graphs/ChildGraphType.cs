public class ChildGraphType :
    EfObjectGraphType<IntegrationDbContext, ChildEntity>
{
    public ChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parentAlias",
            resolve: _ => _.Source.Parent,
            graphType: typeof(ParentGraphType),
            includeNames: ["Parent"]);
        AutoMap();
    }
}