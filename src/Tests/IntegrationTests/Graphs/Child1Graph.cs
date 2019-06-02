using GraphQL.EntityFramework;

public class Child1Graph :
    EfObjectGraphType<MyDbContext, Child1Entity>
{
    public Child1Graph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
    }
}