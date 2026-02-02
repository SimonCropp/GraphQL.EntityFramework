public class FilterBaseGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterBaseEntity>
{
    public FilterBaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
