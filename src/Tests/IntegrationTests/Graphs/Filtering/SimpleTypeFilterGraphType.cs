public class SimpleTypeFilterGraphType :
    EfObjectGraphType<IntegrationDbContext, SimpleTypeFilterEntity>
{
    public SimpleTypeFilterGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
