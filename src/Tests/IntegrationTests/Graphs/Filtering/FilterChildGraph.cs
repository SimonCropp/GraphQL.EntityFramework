using GraphQL.EntityFramework;

public class FilterChildGraph :
    EfObjectGraphType<FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}