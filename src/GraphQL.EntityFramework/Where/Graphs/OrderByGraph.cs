class OrderByGraph :
    InputObjectGraphType<OrderBy>
{
    public OrderByGraph()
    {
        Name = nameof(OrderBy);
        Field(_ => _.Path);
        Field(_ => _.Descending, true);
    }
}