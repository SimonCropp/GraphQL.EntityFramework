using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("FilterChild")]
public class FilterChildGraph :
    EfObjectGraphType<IntegrationDbContext, FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}