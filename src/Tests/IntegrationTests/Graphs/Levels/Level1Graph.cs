using GraphQL.EntityFramework;

public class Level1Graph :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public Level1Graph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "level2Entity",
            resolve: context => context.Source.Level2Entity);
        AutoMap();
    }
}