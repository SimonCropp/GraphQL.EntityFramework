using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("CustomType")]
public class CustomTypeGraph :
    EfObjectGraphType<IntegrationDbContext, CustomTypeEntity>
{
    public CustomTypeGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}