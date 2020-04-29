using GraphQL.EntityFramework;

public class CustomTypeGraph :
    EfObjectGraphType<IntegrationDbContext, CustomTypeEntity>
{
    public CustomTypeGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}