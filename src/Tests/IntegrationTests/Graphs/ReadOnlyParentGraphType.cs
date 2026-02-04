public class ReadOnlyParentGraphType :
    EfObjectGraphType<IntegrationDbContext, ReadOnlyParentEntity>
{
    public ReadOnlyParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
