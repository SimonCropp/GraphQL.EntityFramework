/// <summary>
/// The arguments of a navigation list or connection field that are applied inside the collection
/// subquery, so the database evaluates them: ids, where and orderBy. Skip, take and the connection
/// paging stay with the field's resolver, which applies them to the loaded collection.
/// </summary>
record NavigationArguments(
    GraphQLField Field,
    IReadOnlyCollection<WhereExpression> Wheres,
    IReadOnlyCollection<OrderBy> OrderBys,
    string[]? Ids);
