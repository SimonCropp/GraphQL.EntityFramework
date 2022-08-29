class OrderByGraph :
    InputObjectGraphType<OrderBy>
{
    public OrderByGraph()
    {
        Name = nameof(OrderBy);
        Field(x => x.Path);
        Field(x => x.Descending, true);
    }
}