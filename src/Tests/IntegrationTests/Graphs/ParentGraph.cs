using GraphQL.EntityFramework;

public class ParentGraph :
    EfObjectGraphType<ParentEntity>
{
    public ParentGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationField<ChildEntity>(name: "children",
            resolve: context => context.Source.Children, graphType: typeof(ChildGraph));
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            typeof(ChildGraph),
            includeNames: new[] { "Children"});
    }
}