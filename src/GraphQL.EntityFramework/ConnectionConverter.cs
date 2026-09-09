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

    public static Connection<T> ApplyConnectionContext<T>(List<T> list, int? first, int? after, int? last, int? before)
        where T : class
    {
        if (last is null)
        {
            return First(list, first.GetValueOrDefault(0), after, before, list.Count);
        }

        return Last(list, last.Value, after, before, list.Count);
    }

    static Connection<T> First<T>(List<T> list, int first, int? after, int? before, int count)
        where T : class
    {
        int skip;
        if (before is null)
        {
            // 'after' is an exclusive cursor, so start strictly after it.
            // Matches the IQueryable overload below.
            skip = after + 1 ?? 0;
        }
        else
        {
            skip = Math.Max(before.Value - first, 0);
        }

        return Range(list, skip, first, count);
    }

    static Connection<T> Last<T>(List<T> list, int last, int? after, int? before, int count)
        where T : class
    {
        var (skip, take) = LastRange(last, after, before, count);

        return Range(list, skip, take, count, true);
    }

    /// <summary>
    /// Resolve the skip/take for a `last` page. When `last` exceeds the number of items available before
    /// the cursor, the start of the range clamps to zero and the page shrinks to what is available.
    /// Without the clamp a negative skip reaches the database as a negative SQL OFFSET.
    /// </summary>
    static (int skip, int take) LastRange(int last, int? after, int? before, int count)
    {
        if (after is not null)
        {
            // last after
            return (after.Value + 1, last);
        }

        // last before
        var start = before.GetValueOrDefault(count);
        var skip = start - last;
        if (skip < 0)
        {
            return (0, Math.Max(start, 0));
        }

        return (skip, last);
    }

    static Connection<T> Range<T>(
        List<T> list,
        int skip,
        int take,
        int count,
        bool reverse = false)
        where T : class
    {
        var page = list.Skip(skip).Take(take).ToList();
        if (reverse)
        {
           page.Reverse();
        }
        return Build(skip, take, count, page);
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
        if (last is null)
        {
            return await First(queryable, first.GetValueOrDefault(0), after, before, count, context, filters, cancel, data);
        }

        return await Last(queryable, last.Value, after, before, count, context, filters, cancel, data);
    }

    static Task<Connection<TItem>> First<TDbContext, TSource, TItem>(
        IQueryable<TItem> queryable,
        int first,
        int? after,
        int? before,
        int count,
        IResolveFieldContext<TSource> context,
        Filters<TDbContext>? filters,
        Cancel cancel,
        TDbContext data)
        where TItem : class
        where TDbContext : DbContext
    {
        int skip;
        if (before is null)
        {
            skip = after + 1 ?? 0;
        }
        else
        {
            skip = Math.Max(before.Value - first, 0);
        }

        return Range(queryable, skip, first, count, context, filters, cancel, data);
    }

    static Task<Connection<TItem>> Last<TDbContext, TSource, TItem>(
        IQueryable<TItem> queryable,
        int last,
        int? after,
        int? before,
        int count,
        IResolveFieldContext<TSource> context,
        Filters<TDbContext>? filters,
        Cancel cancel,
        TDbContext data)
        where TItem : class
        where TDbContext : DbContext
    {
        var (skip, take) = LastRange(last, after, before, count);

        return Range(queryable, skip, take, count, context, filters, cancel, data);
    }

    static async Task<Connection<TItem>> Range<TDbContext, TSource, TItem>(
        IQueryable<TItem> queryable,
        int skip,
        int take,
        int count,
        IResolveFieldContext<TSource> context,
        Filters<TDbContext>? filters,
        Cancel cancel,
        TDbContext data)
        where TItem : class
        where TDbContext : DbContext
    {
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
                HasNextPage = count > take + skip,
                HasPreviousPage = skip > 0,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(count - 1, take - 1 + skip).ToString()
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
