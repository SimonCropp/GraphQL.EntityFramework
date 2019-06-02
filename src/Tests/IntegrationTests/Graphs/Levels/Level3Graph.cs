using GraphQL.EntityFramework;

public class Level3Graph :
    EfObjectGraphType<MyDbContext, Level3Entity>
{
    public Level3Graph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}