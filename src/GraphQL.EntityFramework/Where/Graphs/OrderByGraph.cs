using GraphQL.Types;

class OrderByGraph :
    InputObjectGraphType<OrderBy>
{
    public OrderByGraph()
    {
        Field(x => x.Path);
        Field(x => x.Descending, true);
    }
}