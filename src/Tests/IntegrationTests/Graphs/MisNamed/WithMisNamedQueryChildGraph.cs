using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraph :
    EfObjectGraphType<MyDbContext, WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "parent",
            resolve: context => context.Source.Parent);
    }
}