using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(Entity2))]
public class Entity2Graph :
    EfObjectGraphType<DbContext2, Entity2>
{
    public Entity2Graph(IEfGraphQLService<DbContext2> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}