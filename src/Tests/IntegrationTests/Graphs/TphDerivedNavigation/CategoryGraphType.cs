public class CategoryGraphType :
    EfObjectGraphType<IntegrationDbContext, CategoryEntity>
{
    public CategoryGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
