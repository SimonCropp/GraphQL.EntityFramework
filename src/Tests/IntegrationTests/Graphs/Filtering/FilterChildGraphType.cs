using GraphQL.EntityFramework;

public class FilterChildGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterChildEntity>
{
    public FilterChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}