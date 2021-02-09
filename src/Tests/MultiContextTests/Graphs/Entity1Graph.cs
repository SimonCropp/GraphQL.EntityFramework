using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(Entity1))]
public class Entity1Graph :
    EfObjectGraphType<DbContext1, Entity1>
{
    public Entity1Graph(IEfGraphQLService<DbContext1> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}