using GraphQL.EntityFramework;

public class Entity2GraphType :
    EfObjectGraphType<DbContext2, Entity2>
{
    public Entity2GraphType(IEfGraphQLService<DbContext2> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}