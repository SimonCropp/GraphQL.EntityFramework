public class StringEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, StringEntity>
{
    public StringEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}