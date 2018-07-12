using GraphQL.Types;

public class ParentGraph : ObjectGraphType<Parent>
{
    public ParentGraph()
    {
        Field(x => x.Id);
    }
}