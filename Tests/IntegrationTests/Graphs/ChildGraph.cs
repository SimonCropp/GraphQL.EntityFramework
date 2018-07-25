using GraphQL.EntityFramework;

public class ChildGraph : EfObjectGraphType<ChildEntity>
{
    public ChildGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        Field(x => x.Nullable, true);
        AddNavigationField<ParentGraph, ParentEntity>(
            name: "parent",
            resolve: context => context.Source.Parent);
    }
}