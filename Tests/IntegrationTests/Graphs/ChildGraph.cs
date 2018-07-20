using GraphQL.EntityFramework;


public class ChildGraph : EfObjectGraphType<ChildEntity>
{
    public ChildGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        Field(x => x.Nullable,true);
        Field(typeof(ParentGraph), "parent", null, null, x => x.Source.Parent);
    }
}