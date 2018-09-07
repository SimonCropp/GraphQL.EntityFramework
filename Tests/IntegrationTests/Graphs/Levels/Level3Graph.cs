using GraphQL.EntityFramework;

public class Level3Graph : EfObjectGraphType<Level3Entity>
{
    public Level3Graph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}