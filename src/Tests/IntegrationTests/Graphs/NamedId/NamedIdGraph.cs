using GraphQL.EntityFramework;

public class NamedIdGraph :
    EfObjectGraphType<MyDbContext, NamedIdEntity>
{
    public NamedIdGraph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.NamedId);
        Field(x => x.Property);
    }
}