public class EntityGraphType :
    EfObjectGraphType<DependencyDbContext, Entity>
{
    public EntityGraphType(IEfGraphQLService<DependencyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(_ => _.Id);
        Field(_ => _.Property);
    }
}