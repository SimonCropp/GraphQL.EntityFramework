public class FilterDerivedGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterDerivedEntity>
{
    public FilterDerivedGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
