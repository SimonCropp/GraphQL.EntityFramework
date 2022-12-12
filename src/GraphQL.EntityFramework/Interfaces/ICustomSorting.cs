/// <summary>
///     Base service to apply custom sorting.
/// </summary>
/// <typeparam name="TItem">EF entity type</typeparam>
public interface ICustomSorting<TItem>
{
    /// <summary>
    ///     Apply custom sorting to the lists
    /// </summary>
    /// <param name="query">The query to apply sorting to.</param>
    /// <param name="orderBy">The sorting to apply</param>
    /// <param name="isFirst">Indicates if this is a first sort in the list or not</param>
    /// <param name="ordered">Ordered</param>
    /// <returns>The query with sorting applied</returns>
    bool ApplySort(IEnumerable<TItem> query, OrderBy orderBy, bool isFirst, out IOrderedEnumerable<TItem> ordered);

    /// <summary>
    ///     Apply custom sorting to the qureyables
    /// </summary>
    /// <param name="query">The query to apply sorting to.</param>
    /// <param name="orderBy">The sorting to apply</param>
    /// <param name="isFirst">Indicates if this is a first sort in the list or not</param>
    /// <param name="ordered">Ordered</param>
    /// <returns>The query with sorting applied</returns>
    bool ApplySort(IQueryable<TItem> query, OrderBy orderBy, bool isFirst, out IOrderedQueryable<TItem> ordered);

}
