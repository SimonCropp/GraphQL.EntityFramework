using GraphQL.EntityFramework;

public class WithNullableGraph :
    EfObjectGraphType<WithNullableEntity>
{
    public WithNullableGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Nullable,true);
    }
}