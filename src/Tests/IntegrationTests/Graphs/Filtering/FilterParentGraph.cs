using GraphQL.EntityFramework;

public class FilterParentGraph :
    EfObjectGraphType<IntegrationDbContext, FilterParentEntity>
{
    public FilterParentGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] {"Children"});
        AutoMap();
    }
}