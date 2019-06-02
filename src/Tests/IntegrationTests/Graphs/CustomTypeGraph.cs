using GraphQL.EntityFramework;

public class CustomTypeGraph :
    EfObjectGraphType<IntegrationDbContext, CustomTypeEntity>
{
    public CustomTypeGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}