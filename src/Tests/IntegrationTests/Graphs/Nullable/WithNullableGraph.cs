using GraphQL.EntityFramework;

public class WithNullableGraph :
    EfObjectGraphType<IntegrationDbContext, WithNullableEntity>
{
    public WithNullableGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}