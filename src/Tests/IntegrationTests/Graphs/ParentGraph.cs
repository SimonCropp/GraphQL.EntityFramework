using GraphQL.EntityFramework;

public class ParentGraph :
    EfObjectGraphType<ParentEntity>
{
    public ParentGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        AddNavigationListField(
            name: "children",
            resolve: context => context.Source.Children);
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: context => context.Source.Children,
            includeNames: new[] { "Children"});
    }
}