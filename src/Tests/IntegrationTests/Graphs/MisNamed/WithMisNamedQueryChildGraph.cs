using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraph :
    EfObjectGraphType<WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(name: "parent",
            resolve: context => context.Source.Parent, graphType: typeof(WithMisNamedQueryParentGraph));
    }
}