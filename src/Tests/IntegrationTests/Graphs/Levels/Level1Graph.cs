using GraphQL.EntityFramework;

public class Level1Graph :
    EfObjectGraphType<Level1Entity>
{
    public Level1Graph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            typeof(Level2Graph),
            name: "level2Entity",
            resolve: context => context.Source.Level2Entity);
    }
}