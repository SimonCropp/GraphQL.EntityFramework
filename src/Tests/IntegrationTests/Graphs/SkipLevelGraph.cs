public class SkipLevelGraph :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public SkipLevelGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "level3Entity",
            resolve: _ => _.Source.Level2Entity?.Level3Entity,
            graphType: typeof(Level3GraphType),
            includeNames: [ "Level2Entity.Level3Entity" ]);
        AutoMap();
    }
}