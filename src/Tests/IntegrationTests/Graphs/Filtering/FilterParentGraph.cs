using GraphQL.EntityFramework;

public class FilterParentGraph :
    EfObjectGraphType<FilterParentEntity>
{
    public FilterParentGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationField<FilterChildGraph, FilterChildEntity>(
            name: "children",
            resolve: context => context.Source.Children);
        AddNavigationConnectionField<FilterChildGraph, FilterChildEntity>(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] {"Children"});
    }
}