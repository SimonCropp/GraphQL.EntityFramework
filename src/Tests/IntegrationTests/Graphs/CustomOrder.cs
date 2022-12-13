namespace Tests.IntegrationTests.Graphs;
class CustomOrder : ICustomSorting<ParentEntity>
{
    public bool ApplySort(IEnumerable<ParentEntity> query, OrderBy orderBy, IResolveFieldContext context, bool isFirst, out IOrderedEnumerable<ParentEntity> ordered) =>
        throw new NotImplementedException();

    /// <summary>
    ///    Apply custom sorting to the qureyables for te3st puprpose. Expression maybe be way more coplex in production
    /// </summary>
    /// <param name="query"></param>
    /// <param name="orderBy"></param>
    /// <param name="context"></param>
    /// <param name="isFirst"></param>
    /// <param name="ordered"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool ApplySort(IQueryable<ParentEntity> query, OrderBy orderBy, IResolveFieldContext context, bool isFirst, out IOrderedQueryable<ParentEntity> ordered)
    {
        if (orderBy.Path.ToLower() != "childrensum")
        {
            ordered = query.OrderBy(x => x.Id);
            return false;
        }
        var direction = orderBy.Descending ? -1 : 1;
        if (isFirst)
        {
            ordered = query.OrderBy(x => x.Children.Sum(y => y.Decimal) * direction);
            return true;
        }
        if (query is not IOrderedQueryable<ParentEntity> orderedQuery)
        {
            throw new Exception("Expected IOrderedQueryable");
        }
        ordered = orderedQuery.ThenBy(x => x.Children.Sum(y => y.Decimal) * direction);
        return true;
    }
}
