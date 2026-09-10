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
        // long, since a large `first` makes take + skip overflow and wrap negative
        return Build(skip, count, skip > 0, count > (long) take + skip, page);
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

        int skip;
        int? count = null;
        bool hasPreviousPage;
        bool hasNextPage;
        List<TItem> rows;
        if (NeedsCount(context, last, before))
        {
            count = await queryable.CountAsync(cancel);
            cancel.ThrowIfCancellationRequested();
            int take;
            (skip, take) = Window(first, after, last, before, count.Value);
            var page = queryable.Skip(skip).Take(take);
            QueryLogger.Write(page);
            rows = await page.ToListAsync(cancel);
            hasPreviousPage = skip > 0;
            // long, since a large `first` makes take + skip overflow and wrap negative
            hasNextPage = count > (long) take + skip;
        }
        else
        {
            // The window is bounded from the start only, so it needs no count to place it. The
            // count clamped the offset to the end; past it the page query reads an empty page.
            skip = after + 1 ?? 0;
            var page = queryable.Skip(skip);
            if (first is not null)
            {
                // One row past the page says whether a next page exists
                page = page.Take(Peek(first.Value));
            }

            QueryLogger.Write(page);
            rows = await page.ToListAsync(cancel);
            hasNextPage = first is not null && rows.Count > first.Value;
            // Rows on the page prove rows before it. An empty page proves nothing, and paging
            // forward the spec allows false when that is unknown.
            hasPreviousPage = skip > 0 && rows.Count > 0;
            if (hasNextPage)
            {
                rows.RemoveAt(rows.Count - 1);
            }
        }

        IEnumerable<TItem> result = rows;
        if (filters != null)
        {
            result = await filters.ApplyFilter(result, context.UserContext, data, context.User);
        }

        cancel.ThrowIfCancellationRequested();
        return Build(skip, count, hasPreviousPage, hasNextPage, result);
    }

    /// <summary>
    /// The page size plus the one row read past it, capped so the largest page size does not wrap.
    /// </summary>
    static int Peek(int first) =>
        first == int.MaxValue ? first : first + 1;

    /// <summary>
    /// Whether the count query has to run: when totalCount is selected, or when the window is
    /// bounded from the end, by last or before, so the count is needed to place it. The page
    /// info alone does not need it, since hasNextPage is answered by the row read past the page.
    /// A connection selecting edges, items and page info otherwise paid a second round trip, a
    /// COUNT over the whole filtered set, for a number nothing read. The selection is unknown
    /// for a context built outside an execution, which counts.
    /// </summary>
    static bool NeedsCount(IResolveFieldContext context, int? last, int? before)
    {
        if (last is not null ||
            before is not null)
        {
            return true;
        }

        var subFields = context.SubFields;
        if (subFields is null)
        {
            return true;
        }

        foreach (var (field, _) in subFields.Values)
        {
            if (field.Name.Value.Equals("totalCount"))
            {
                return true;
            }
        }

        return false;
    }

    /// <param name="count">Null when the count query was skipped, which only happens when the total count was not selected.</param>
    static Connection<T> Build<T>(int skip, int? count, bool hasPreviousPage, bool hasNextPage, IEnumerable<T> result)
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
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage,
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
