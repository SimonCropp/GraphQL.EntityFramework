using GraphQL.EntityFramework;

public class WithNullableGraph :
    EfObjectGraphType<IntegrationDbContext, WithNullableEntity>
{
    public WithNullableGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Nullable,true);
    }
}