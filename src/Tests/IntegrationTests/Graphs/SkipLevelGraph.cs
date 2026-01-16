public class SkipLevelGraph :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public SkipLevelGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "level3Entity",
            projection: _ => _.Level2Entity!.Level3Entity,
            resolve: ctx => ctx.Projection,
            graphType: typeof(Level3GraphType));
        AutoMap();
    }
}