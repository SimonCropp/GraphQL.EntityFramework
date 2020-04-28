using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraph :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parent",
            resolve: context => context.Source.Parent);
        AutoMap();
    }
}