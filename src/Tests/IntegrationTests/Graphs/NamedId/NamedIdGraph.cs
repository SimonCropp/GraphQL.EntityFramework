using GraphQL.EntityFramework;

public class NamedIdGraph :
    EfObjectGraphType<NamedIdEntity>
{
    public NamedIdGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.NamedId);
        Field(x => x.Property);
    }
}