using GraphQL.EntityFramework;

public class ParentGraph : EfObjectGraphType<ParentEntity>
{
    public ParentGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        Field(x => x.Nullable,true);
        AddNavigationField<ChildGraph, ChildEntity>(
            name: "children",
            resolve: context => context.Source.Children);
        AddNavigationConnectionField<ChildGraph, ChildEntity>(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeName: "Children");
    }
}