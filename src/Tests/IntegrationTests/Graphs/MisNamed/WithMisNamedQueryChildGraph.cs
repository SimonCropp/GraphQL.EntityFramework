using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraph :
    EfObjectGraphType<WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            typeof(WithMisNamedQueryParentGraph),
            name: "parent",
            resolve: context => context.Source.Parent);
    }
}