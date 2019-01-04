using GraphQL.EntityFramework;

public class Child1Graph :
    EfObjectGraphType<Child1Entity>
{
    public Child1Graph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
    }
}