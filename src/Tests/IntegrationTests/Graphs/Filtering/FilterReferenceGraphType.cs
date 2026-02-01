public class FilterReferenceGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterReferenceEntity>
{
    public FilterReferenceGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
