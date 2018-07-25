using GraphQL.EntityFramework;

public class SkipLevelGraph : EfObjectGraphType<Level1Entity>
{
    public SkipLevelGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField<Level3Graph, Level3Entity>(
            name: "level3Entity",
            resolve: context => context.Source.Level2Entity.Level3Entity,includeName: "Level2Entity");
    }
}