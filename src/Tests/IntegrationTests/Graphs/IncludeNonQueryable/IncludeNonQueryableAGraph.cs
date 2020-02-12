using GraphQL.EntityFramework;

public class IncludeNonQueryableAGraph :
    EfObjectGraphType<IntegrationDbContext, IncludeNonQueryableA>
{
    public IncludeNonQueryableAGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "includeNonQueryableB",
            resolve: context => context.Source.IncludeNonQueryableB);
    }
}