using GraphQL.EntityFramework;

public class WithNullableGraph :
    EfObjectGraphType<MyDbContext, WithNullableEntity>
{
    public WithNullableGraph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Nullable,true);
    }
}