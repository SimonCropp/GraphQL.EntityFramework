static class ConnectionConverter
{
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
            skip = after.GetValueOrDefault(0);
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
        int skip;
        if (after is null)
        {
            // last before
            skip = before.GetValueOrDefault(count) - last;
        }
        else
        {
            // last after
            skip = after.Value + 1;
        }

        return Range(list, skip, take: last, count, true);
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

    public static Task<Connection<TItem>> ApplyConnectionContext<TSource, TItem>(
        this IQueryable<TItem> queryable,
        int? first,
        string afterString,
        int? last,
        string beforeString,
        IResolveFieldContext<TSource> context,
        Cancel cancel,
        Filters filters)
        where TItem : class
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(queryable, first, after, last, before, context, filters, cancel);
    }

    public static async Task<Connection<TItem>> ApplyConnectionContext<TSource, TItem>(
        IQueryable<TItem> queryable,
        int? first,
        int? after,
        int? last,
        int? before,
        IResolveFieldContext<TSource> context,
        Filters filters,
        Cancel cancel = default)
        where TItem : class
    {
        if (queryable is not IOrderedQueryable<TItem>)
        {
            throw new($"Connections require ordering. Either order the IQueryable being passed to AddQueryConnectionField, or use an orderBy in the query. Field: {context.FieldDefinition.Name}");
        }
        var count = await queryable.CountAsync(cancel);
        cancel.ThrowIfCancellationRequested();
        if (last is null)
        {
            return await First(queryable, first.GetValueOrDefault(0), after, before, count, context, filters, cancel);
        }

        return await Last(queryable, last.Value, after, before, count, context, filters, cancel);
    }

    static Task<Connection<TItem>> First<TSource, TItem>(
        IQueryable<TItem> queryable,
        int first,
        int? after,
        int? before,
        int count,
        IResolveFieldContext<TSource> context,
        Filters filters,
        Cancel cancel)
        where TItem : class
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

        return Range(queryable, skip, first, count, context, filters, cancel);
    }

    static Task<Connection<TItem>> Last<TSource, TItem>(
        IQueryable<TItem> queryable,
        int last,
        int? after,
        int? before,
        int count,
        IResolveFieldContext<TSource> context,
        Filters filters,
        Cancel cancel)
        where TItem : class
    {
        int skip;
        if (after is null)
        {
            // last before
            skip = before.GetValueOrDefault(count) - last;
        }
        else
        {
            // last after
            skip = after.Value + 1;
        }

        return Range(queryable, skip, take: last, count, context, filters, cancel);
    }

    static async Task<Connection<TItem>> Range<TSource, TItem>(
        IQueryable<TItem> queryable,
        int skip,
        int take,
        int count,
        IResolveFieldContext<TSource> context,
        Filters filters,
        Cancel cancel)
        where TItem : class
    {
        var page = queryable.Skip(skip).Take(take);
        QueryLogger.Write(page);
        IEnumerable<TItem> result = await page.ToListAsync(cancel);
        result = await filters.ApplyFilter(result, context.UserContext, context.User);

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
                HasPreviousPage = skip > 0 && take < count,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(count - 1, take - 1 + skip).ToString()
            }
        };
    }

    static void Parse(string? afterString, string? beforeString, out int? after, out int? before)
    {
        after = null;
        if (afterString is not null)
        {
            after = int.Parse(afterString);
        }

        before = null;
        if (beforeString is not null)
        {
            before = int.Parse(beforeString);
        }
    }
}