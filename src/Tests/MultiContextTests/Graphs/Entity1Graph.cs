using GraphQL.EntityFramework;

public class Entity1Graph :
    EfObjectGraphType<DbContext1, Entity1>
{
    public Entity1Graph(IEfGraphQLService<DbContext1> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}