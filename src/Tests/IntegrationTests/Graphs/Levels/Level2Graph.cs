using GraphQL.EntityFramework;

public class Level2Graph :
    EfObjectGraphType<MyDbContext, Level2Entity>
{
    public Level2Graph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "level3Entity",
            resolve: context => context.Source.Level3Entity);
    }
}