using GraphQL.EntityFramework;

public class FilterParentGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterParentEntity>
{
    public FilterParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] {"Children"});
        AutoMap();
    }
}