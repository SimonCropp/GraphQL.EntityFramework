using GraphQL.EntityFramework;

public class CustomTypeGraphType :
    EfObjectGraphType<IntegrationDbContext, CustomTypeEntity>
{
    public CustomTypeGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}