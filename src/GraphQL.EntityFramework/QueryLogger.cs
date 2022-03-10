using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

public static class QueryLogger
{
    static Action<string>? log;

    public static void Enable(Action<string> log) =>
        QueryLogger.log = log;

    internal static void Write(IQueryable queryable) =>
        log?.Invoke(queryable.ToQueryString());
}