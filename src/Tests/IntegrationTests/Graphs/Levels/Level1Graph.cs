using GraphQL.EntityFramework;

public class Level1Graph :
    EfObjectGraphType<MyDbContext, Level1Entity>
{
    public Level1Graph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "level2Entity",
            resolve: context => context.Source.Level2Entity);
    }
}