using GraphQL.EntityFramework;

public class EntityGraphType :
    EfObjectGraphType<DependencyDbContext, Entity>
{
    public EntityGraphType(IEfGraphQLService<DependencyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}