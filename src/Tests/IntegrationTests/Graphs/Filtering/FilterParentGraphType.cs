public class FilterParentGraphType :
    EfObjectGraphType<IntegrationDbContext, FilterParentEntity>
{
    public FilterParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: _ => _.Source.Children,
            includeNames: ["Children"]);
        AutoMap();
    }
}