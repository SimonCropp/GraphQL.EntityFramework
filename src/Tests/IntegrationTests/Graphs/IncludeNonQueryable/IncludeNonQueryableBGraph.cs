using GraphQL.EntityFramework;

public class IncludeNonQueryableBGraph :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableB>
{
    public IncludeNonQueryableBGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}