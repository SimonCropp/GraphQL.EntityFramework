using GraphQL.EntityFramework;

public class Level2Graph :
    EfObjectGraphType<Level2Entity>
{
    public Level2Graph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "level3Entity",
            resolve: context => context.Source.Level3Entity);
    }
}