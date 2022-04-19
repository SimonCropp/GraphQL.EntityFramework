using GraphQL.EntityFramework;

public class IncludeNonQueryableBGraphType :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableB>
{
    public IncludeNonQueryableBGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}