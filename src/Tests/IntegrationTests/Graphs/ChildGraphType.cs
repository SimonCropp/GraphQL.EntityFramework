using GraphQL.EntityFramework;

public class ChildGraphType :
    EfObjectGraphType<IntegrationDbContext, ChildEntity>
{
    public ChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parentAlias",
            resolve: context => context.Source.Parent,
            graphType: typeof(ParentGraphType),
            includeNames: new[] {"Parent"});
        AutoMap();
    }
}