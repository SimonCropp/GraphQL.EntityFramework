using GraphQL.EntityFramework;

public class ParentGraph : EfObjectGraphType<ParentEntity>
{
    public ParentGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationField<ChildGraph, ChildEntity>(
            name: "children",
            resolve: context => context.Source.Children);
        AddNavigationConnectionField<ChildGraph, ChildEntity>(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] { "Children"});
    }
}