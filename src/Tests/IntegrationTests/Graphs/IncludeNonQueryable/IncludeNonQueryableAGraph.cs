using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(IncludeNonQueryableA))]
public class IncludeNonQueryableAGraph :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableA>
{
    public IncludeNonQueryableAGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}