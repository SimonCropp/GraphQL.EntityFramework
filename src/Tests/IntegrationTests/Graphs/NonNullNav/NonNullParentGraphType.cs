public class NonNullParentGraphType :
    EfObjectGraphType<IntegrationDbContext, NonNullParentEntity>
{
    public NonNullParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}