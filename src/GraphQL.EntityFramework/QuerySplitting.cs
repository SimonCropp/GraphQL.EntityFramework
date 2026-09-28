/// <summary>
/// A resolver can call AsSplitQuery or AsSingleQuery for the collections a request might select.
/// When the select projection loads no collection they do nothing, and EF ignores them, so they
/// are removed.
/// </summary>
static class QuerySplitting
{
    public static IQueryable<T> Remove<T>(IQueryable<T> queryable)
    {
        var expression = Remove(queryable.Expression);
        if (expression == queryable.Expression)
        {
            return queryable;
        }

        return queryable.Provider.CreateQuery<T>(expression);
    }

    static Expression Remove(Expression expression)
    {
        // Queryable operators, and extensions such as AsSplitQuery, take the query they build on
        // as their first argument. Unlike ordering, splitting never decides which rows a Skip or
        // Take keeps, so the whole chain is walked.
        if (expression is not MethodCallExpression { Object: null, Arguments.Count: > 0 } methodCall ||
            !typeof(IQueryable).IsAssignableFrom(methodCall.Arguments[0].Type))
        {
            return expression;
        }

        if (methodCall.Method.Name is "AsSplitQuery" or "AsSingleQuery")
        {
            return Remove(methodCall.Arguments[0]);
        }

        var source = Remove(methodCall.Arguments[0]);
        if (source == methodCall.Arguments[0])
        {
            return expression;
        }

        return methodCall.Update(methodCall.Object, [source, .. methodCall.Arguments.Skip(1)]);
    }
}
