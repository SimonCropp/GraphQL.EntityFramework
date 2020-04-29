using GraphQL.EntityFramework;

public class ChildGraph :
    EfObjectGraphType<IntegrationDbContext, ChildEntity>
{
    public ChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parentAlias",
            resolve: context => context.Source.Parent,
            graphType: typeof(ParentGraph),
            includeNames: new[] {"Parent"});
        AutoMap();
    }
}