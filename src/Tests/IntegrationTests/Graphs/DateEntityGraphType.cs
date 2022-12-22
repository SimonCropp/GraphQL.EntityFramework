public class DateEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, DateEntity>
{
    public DateEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}