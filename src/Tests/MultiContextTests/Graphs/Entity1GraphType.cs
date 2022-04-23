using GraphQL.EntityFramework;

public class Entity1GraphType :
    EfObjectGraphType<DbContext1, Entity1>
{
    public Entity1GraphType(IEfGraphQLService<DbContext1> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}