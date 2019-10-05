using GraphQL.EntityFramework;

public class SkipLevelGraph :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public SkipLevelGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "level3Entity",
            resolve: context => context.Source.Level2Entity?.Level3Entity,
            graphType: typeof(Level3Graph),
            includeNames: new[] { "Level2Entity.Level3Entity"});
    }
}