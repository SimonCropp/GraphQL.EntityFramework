public class TimeEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, TimeEntity>
{
    public TimeEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}