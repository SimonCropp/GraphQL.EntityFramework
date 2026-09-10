static class ConnectionConverter
{
    internal static bool HasOrderingInExpressionTree(Expression expression)
    {
        if (expression is MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;
            if (methodName is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending")
            {
                return true;
            }

            foreach (var arg in methodCall.Arguments)
            {
                if (HasOrderingInExpressionTree(arg))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static Connection<T> ApplyConnectionContext<T>(List<T> list, int? first, string? afterString, int? last, string? beforeString)
        where T : class
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(list, first, after, last, before);
    }

    /// <summary>
    /// With neither `first` nor `last`, and no page size on the field, `first` defaulted to zero and
    /// the connection came back with a total count but no edges. It now returns everything.
    /// </summary>
    public static Connection<T> ApplyConnectionContext<T>(List<T> list, int? first, int? after, int? last, int? before)
        where T : class
    {
        var count = list.Count;
        var (skip, take) = Window(first, after, last, before, count);
        var page = list.Skip(skip).Take(take);
        return Build(skip, take, count, page);
    }

    /// <summary>
    /// The skip and take for a page, in the order the Relay spec applies the arguments: `after`
    /// and `before` bound the window, `first` keeps the start of it, then `last` keeps the end.
    /// Cursors are indexes, `after` and `before` exclusive. A page never starts before the
    /// window, so `last` past the start clamps rather than reaching the database as a negative
    /// offset, and the edges keep their order whichever end the page was taken from.
    /// </summary>
    static (int skip, int take) Window(int? first, int? after, int? last, int? before, int count)
    {
        var start = after + 1 ?? 0;

        // The common page, first after, takes the page size as is, so the query does not
        // change with the count
        if (before is null &&
            last is null &&
            first is not null)
        {
            return (Math.Min(start, count), first.Value);
        }

        var end = Math.Min(before ?? count, count);
        start = Math.Min(start, end);

        if (first is not null)
        {
            end = Math.Min(end, start + first.Value);
        }

        if (last is not null)
        {
            start = Math.Max(start, end - last.Value);
        }

        return (start, end - start);
    }

    public static Task<Connection<TItem>> ApplyConnectionContext<TDbContext, TSource, TItem>(
        this IQueryable<TItem> queryable,
        int? first,
        string afterString,
        int? last,
        string beforeString,
        IResolveFieldContext<TSource> context,
        Cancel cancel,
        Filters<TDbContext>? filters,
        TDbContext data)
        where TItem : class
        where TDbContext : DbContext
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(queryable, first, after, last, before, context, filters, cancel, data);
    }

    public static async Task<Connection<TItem>> ApplyConnectionContext<TDbContext, TSource, TItem>(
        IQueryable<TItem> queryable,
        int? first,
        int? after,
        int? last,
        int? before,
        IResolveFieldContext<TSource> context,
        Filters<TDbContext>? filters,
        Cancel cancel,
        TDbContext data)
        where TItem : class
        where TDbContext : DbContext
    {
        if (queryable is not IOrderedQueryable<TItem> && !HasOrderingInExpressionTree(queryable.Expression))
        {
            throw new($"Connections require ordering. Either order the IQueryable being passed to AddQueryConnectionField, or use an orderBy in the query. Field: {context.FieldDefinition.Name}");
        }
        var count = await queryable.CountAsync(cancel);
        cancel.ThrowIfCancellationRequested();
        var (skip, take) = Window(first, after, last, before, count);
        var page = queryable.Skip(skip).Take(take);
        QueryLogger.Write(page);
        IEnumerable<TItem> result = await page.ToListAsync(cancel);
        if (filters != null)
        {
            result = await filters.ApplyFilter(result, context.UserContext, data, context.User);
        }

        cancel.ThrowIfCancellationRequested();
        return Build(skip, take, count, result);
    }

    static Connection<T> Build<T>(int skip, int take, int count, IEnumerable<T> result)
    {
        var edges = result
            .Select((item, index) =>
                new Edge<T>
                {
                    Cursor = (index + skip).ToString(),
                    Node = item
                })
            .ToList();

        return new()
        {
            TotalCount = count,
            Edges = edges,
            PageInfo = new()
            {
                // long, since a large `first` makes take + skip overflow and wrap negative
                HasNextPage = count > (long) take + skip,
                HasPreviousPage = skip > 0,
                // Null when there are no edges, as the spec has it. The edges are used rather
                // than the window since filters can remove items after the query.
                StartCursor = edges.FirstOrDefault()?.Cursor,
                EndCursor = edges.LastOrDefault()?.Cursor
            }
        };
    }

    static void Parse(string? afterString, string? beforeString, out int? after, out int? before)
    {
        after = ParseCursor(afterString, "after");
        before = ParseCursor(beforeString, "before");
    }

    /// <summary>
    /// Cursors are client supplied, so a malformed one is bad input rather than a bug. Parsing with
    /// int.Parse surfaced a raw FormatException or OverflowException, and a negative value parsed
    /// happily and then reached the database as a negative SQL OFFSET.
    /// </summary>
    static int? ParseCursor(string? value, string name)
    {
        if (value is null)
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cursor))
        {
            throw new($"The `{name}` cursor must be an integer. Value: {value}");
        }

        if (cursor < 0)
        {
            throw new($"The `{name}` cursor cannot be negative. Value: {value}");
        }

        return cursor;
    }
}
