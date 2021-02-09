using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(Entity))]
public class EntityGraph :
    EfObjectGraphType<DependencyDbContext, Entity>
{
    public EntityGraph(IEfGraphQLService<DependencyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}