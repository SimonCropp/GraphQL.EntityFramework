using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(IncludeNonQueryableB))]
public class IncludeNonQueryableBGraph :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableB>
{
    public IncludeNonQueryableBGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}