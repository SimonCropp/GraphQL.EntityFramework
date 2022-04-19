using GraphQL.EntityFramework;

public class WithNullableGraphType :
    EfObjectGraphType<IntegrationDbContext, WithNullableEntity>
{
    public WithNullableGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}