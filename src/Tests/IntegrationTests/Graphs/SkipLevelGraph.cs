using GraphQL.EntityFramework;

public class SkipLevelGraph :
    EfObjectGraphType<Level1Entity>
{
    public SkipLevelGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            typeof(Level3Graph),
            name: "level3Entity",
            resolve: context => context.Source.Level2Entity.Level3Entity,
            includeNames: new[] { "Level2Entity.Level3Entity"});
    }
}