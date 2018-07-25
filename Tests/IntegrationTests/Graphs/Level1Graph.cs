using GraphQL.EntityFramework;

public class Level1Graph : EfObjectGraphType<Level1Entity>
{
    public Level1Graph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField<Level2Graph, Level2Entity>(
            name: "level2Entity",
            resolve: context => context.Source.Level2Entity);
    }
}