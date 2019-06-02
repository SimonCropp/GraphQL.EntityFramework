using GraphQL.EntityFramework;

public class Entity2Graph :
    EfObjectGraphType<DbContext2, Entity2>
{
    public Entity2Graph(IEfGraphQLService<DbContext2> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}