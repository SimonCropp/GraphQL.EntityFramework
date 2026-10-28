/// <summary>
/// A resolver can call AsSplitQuery or AsSingleQuery for the collections a request might select.
/// When the select projection loads no collection they do nothing, and EF ignores them, so they
/// are removed.
/// </summary>
static class QuerySplitting
{
    /// <summary>
    /// Whether the resolver chose a splitting behavior with AsSplitQuery or AsSingleQuery, which
    /// is then left as it is.
    /// </summary>
    public static bool IsChosen(Expression expression)
    {
        var current = expression;
        while (current is MethodCallExpression { Object: null, Arguments.Count: > 0 } call &&
               typeof(IQueryable).IsAssignableFrom(call.Arguments[0].Type))
        {
            if (call.Method.Name is "AsSplitQuery" or "AsSingleQuery")
            {
                return true;
            }

            current = call.Arguments[0];
        }

        return false;
    }

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
        if (expression is not MethodCallExpression { Object: null, Arguments.Count: > 0 } call ||
            !typeof(IQueryable).IsAssignableFrom(call.Arguments[0].Type))
        {
            return expression;
        }

        if (call.Method.Name is "AsSplitQuery" or "AsSingleQuery")
        {
            // ReSharper disable once TailRecursiveCall
            return Remove(call.Arguments[0]);
        }

        var source = Remove(call.Arguments[0]);
        if (source == call.Arguments[0])
        {
            return expression;
        }

        return call.Update(call.Object, [source, .. call.Arguments.Skip(1)]);
    }
}
