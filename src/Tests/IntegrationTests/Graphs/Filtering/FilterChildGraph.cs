using GraphQL.EntityFramework;

public class FilterChildGraph :
    EfObjectGraphType<IntegrationDbContext, FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}