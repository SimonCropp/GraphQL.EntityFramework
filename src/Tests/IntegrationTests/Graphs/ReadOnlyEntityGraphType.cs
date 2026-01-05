public class ReadOnlyEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, ReadOnlyEntity>
{
    public ReadOnlyEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
