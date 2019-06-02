using GraphQL.EntityFramework;

public class FilterChildGraph :
    EfObjectGraphType<MyDbContext, FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}