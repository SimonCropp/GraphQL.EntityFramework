public class CustomOrderChildGraphType :
    EfObjectGraphType<IntegrationDbContext, CustomOrderChildEntity>
{
    public CustomOrderChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}