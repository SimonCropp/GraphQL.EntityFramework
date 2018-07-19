using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

static class ConnectionConverter
{
    public static Connection<TReturn> ApplyConnectionContext<TReturn>(List<TReturn> list, int? first, string afterString, int? last, string beforeString)
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(list, first, after, last, before);
    }

    public static Connection<TReturn> ApplyConnectionContext<TReturn>(List<TReturn> list, int? first, int? after, int? last, int? before)
    {
        if (last == null)
        {
            return First(list, first.GetValueOrDefault(0), after, before, list.Count);
        }

        return Last(list, last.Value, after, before, list.Count);
    }

    static Connection<TReturn> First<TReturn>(List<TReturn> list, int first, int? after, int? before, int count)
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

    static Connection<TReturn> Last<TReturn>(List<TReturn> list, int last, int? after, int? before, int count)
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

    static Connection<TReturn> Range<TReturn>(List<TReturn> list, int skip, int take, int count, bool reverse = false)
    {
        var page = list.Skip(skip).Take(take).ToList();
        return Build(skip, take, count, reverse, page);
    }

    public static Task<Connection<TReturn>> ApplyConnectionContext<TReturn>(this IQueryable<TReturn> list, int? first, string afterString, int? last, string beforeString)
    {
        Parse(afterString, beforeString, out var after, out var before);
        return ApplyConnectionContext(list, first, after, last, before);
    }

    public static async Task<Connection<TReturn>> ApplyConnectionContext<TReturn>(IQueryable<TReturn> list, int? first, int? after, int? last, int? before)
    {
        var count = await list.CountAsync().ConfigureAwait(false);
        if (last == null)
        {
            return await First(list, first.GetValueOrDefault(0), after, before, count).ConfigureAwait(false);
        }

        return await Last(list, last.Value, after, before, count).ConfigureAwait(false);
    }

    static Task<Connection<TReturn>> First<TReturn>(IQueryable<TReturn> list, int first, int? after, int? before, int count)
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

    static Task<Connection<TReturn>> Last<TReturn>(IQueryable<TReturn> list, int last, int? after, int? before, int count)
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

    static async Task<Connection<TReturn>> Range<TReturn>(IQueryable<TReturn> list, int skip, int take, int count, bool reverse = false)
    {
        var page = list.Skip(skip).Take(take);

        var result = await page
            .ToListAsync()
            .ConfigureAwait(false);
        return Build(skip, take, count, reverse, result);
    }

    static Connection<TReturn> Build<TReturn>(int skip, int take, int count, bool reverse, List<TReturn> result)
    {
        var edges = result
            .Select((item, index) =>
                new Edge<TReturn>
                {
                    Cursor = (index + skip).ToString(),
                    Node = item
                })
            .ToList();
        if (reverse)
        {
            edges.Reverse();
        }

        return new Connection<TReturn>
        {
            TotalCount = count,
            Edges = edges,
            PageInfo = new PageInfo
            {
                HasNextPage = count > take + skip,
                HasPreviousPage = skip > 0 && take < count,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(count - 1, take - 1 + skip).ToString(),
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