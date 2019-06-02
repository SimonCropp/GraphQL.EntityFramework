using GraphQL.EntityFramework;

public class FilterChildGraph :
    EfObjectGraphType<IntegrationDbContext, FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}