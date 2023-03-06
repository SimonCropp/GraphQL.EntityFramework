public class EnumEntityGraphType :
    EfObjectGraphType<IntegrationDbContext, EnumEntity>
{
    public EnumEntityGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}