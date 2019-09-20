using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

static class ConnectionConverter
{
    public static Connection<T> ApplyConnectionContext<T>(List<T> list, int? first, string afterString, int? last, string beforeString)
        where T : class
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(list, first, after, last, before);
    }

    public static Connection<T> ApplyConnectionContext<T>(List<T> list, int? first, int? after, int? last, int? before)
        where T : class
    {
        if (last == null)
        {
            return First(list, first.GetValueOrDefault(0), after, before, list.Count);
        }

        return Last(list, last.Value, after, before, list.Count);
    }

    static Connection<T> First<T>(List<T> list, int first, int? after, int? before, int count)
        where T : class
    {
        int skip;
        if (before == null)
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
        if (after == null)
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
        this IQueryable<TItem> list,
        int? first,
        string afterString,
        int? last,
        string beforeString,
        ResolveFieldContext<TSource> context,
        CancellationToken cancellation,
        Filters filters)
        where TItem : class
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(list, first, after, last, before, context, filters, cancellation);
    }

    public static async Task<Connection<TItem>> ApplyConnectionContext<TSource, TItem>(
        IQueryable<TItem> list,
        int? first,
        int? after,
        int? last,
        int? before,
        ResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation = default)
        where TItem : class
    {
        var count = await list.CountAsync(cancellation);
        cancellation.ThrowIfCancellationRequested();
        if (last == null)
        {
            return await First(list, first.GetValueOrDefault(0), after, before, count, context, filters, cancellation);
        }

        return await Last(list, last.Value, after, before, count, context, filters, cancellation);
    }

    static Task<Connection<TItem>> First<TSource, TItem>(
        IQueryable<TItem> list,
        int first,
        int? after,
        int? before,
        int count,
        ResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation)
        where TItem : class
    {
        int skip;
        if (before == null)
        {
            skip = after + 1 ?? 0;
        }
        else
        {
            skip = Math.Max(before.Value - first, 0);
        }

        return Range(list, skip, first, count, context, filters, cancellation);
    }

    static Task<Connection<TItem>> Last<TSource, TItem>(
        IQueryable<TItem> list,
        int last,
        int? after,
        int? before,
        int count,
        ResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation)
        where TItem : class
    {
        int skip;
        if (after == null)
        {
            // last before
            skip = before.GetValueOrDefault(count) - last;
        }
        else
        {
            // last after
            skip = after.Value + 1;
        }

        return Range(list, skip, take: last, count, context, filters, cancellation);
    }

    static async Task<Connection<TItem>> Range<TSource, TItem>(
        IQueryable<TItem> list,
        int skip,
        int take,
        int count,
        ResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation)
        where TItem : class
    {
        var page = list.Skip(skip).Take(take);
        IEnumerable<TItem> result = await page.ToListAsync(cancellation);
        result = await filters.ApplyFilter(result, context.UserContext);

        cancellation.ThrowIfCancellationRequested();
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

        return new Connection<T>
        {
            TotalCount = count,
            Edges = edges,
            PageInfo = new PageInfo
            {
                HasNextPage = count > take + skip,
                HasPreviousPage = skip > 0 && take < count,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(count - 1, take - 1 + skip).ToString()
            }
        };
    }

    static void Parse(string afterString, string beforeString, out int? after, out int? before)
    {
        after = null;
        if (afterString != null)
        {
            after = int.Parse(afterString);
        }

        before = null;
        if (beforeString != null)
        {
            before = int.Parse(beforeString);
        }
    }
}