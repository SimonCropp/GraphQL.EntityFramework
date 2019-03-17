using GraphQL.EntityFramework;

public class FilterParentGraph :
    EfObjectGraphType<FilterParentEntity>
{
    public FilterParentGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationField<FilterChildEntity>(name: "children",
            resolve: context => context.Source.Children, graphType: typeof(FilterChildGraph));
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            typeof(FilterChildGraph),
            includeNames: new[] {"Children"});
    }
}