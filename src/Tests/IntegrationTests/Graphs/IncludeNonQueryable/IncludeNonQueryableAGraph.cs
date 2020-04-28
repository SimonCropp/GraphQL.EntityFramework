using GraphQL.EntityFramework;

public class IncludeNonQueryableAGraph :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableA>
{
    public IncludeNonQueryableAGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "includeNonQueryableB",
            resolve: context => context.Source.IncludeNonQueryableB);
        AutoMap();
    }
}