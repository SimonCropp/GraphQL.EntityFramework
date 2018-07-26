using GraphQL.EntityFramework;

public class WithMisNamedQueryChildGraph : EfObjectGraphType<WithMisNamedQueryChildEntity>
{
    public WithMisNamedQueryChildGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField<WithMisNamedQueryParentGraph, WithMisNamedQueryParentEntity>(
            name: "parent",
            resolve: context => context.Source.Parent);
    }
}