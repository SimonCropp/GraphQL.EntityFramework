using GraphQL.EntityFramework;

public class IncludeNonQueryableAGraphType :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableA>
{
    public IncludeNonQueryableAGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}