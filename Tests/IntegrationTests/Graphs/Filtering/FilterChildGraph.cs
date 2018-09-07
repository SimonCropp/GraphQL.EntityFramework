using GraphQL.EntityFramework;

public class FilterChildGraph : EfObjectGraphType<FilterChildEntity>
{
    public FilterChildGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationField<FilterParentGraph, FilterParentEntity>(
            name: "parent",
            resolve: context =>
            {
                return context.Source.Parent;
            });
        AddNavigationField<FilterParentGraph, FilterParentEntity>(
            name: "parentAlias",
            resolve: context => context.Source.Parent,
            includeNames: new []{"Parent"});
    }
}