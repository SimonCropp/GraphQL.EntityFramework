using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

static class ConnectionConverter
{
    public static Task<Connection<TReturn>> ApplyConnectionContext<TReturn>(IQueryable<TReturn> list, int? first, string afterString, int? last, string beforeString)
        where TReturn : class
    {
        int? after = null;
        if (afterString != null)
        {
            after = int.Parse(afterString);
        }

        int? before = null;
        if (beforeString != null)
        {
            before = int.Parse(beforeString);
        }

        return ApplyConnectionContext(list, first, after, last, before);
    }

    public static async Task<Connection<TReturn>> ApplyConnectionContext<TReturn>(IQueryable<TReturn> list, int? first, int? after, int? last, int? before)
        where TReturn : class
    {
        var totalCount = await list.CountAsync().ConfigureAwait(false);
        if (last == null)
        {
            return await First(list, first.GetValueOrDefault(0), after, before, totalCount);
        }

        return await Last(list, last.Value, after, before, totalCount);
    }

    static Task<Connection<TReturn>> First<TReturn>(IQueryable<TReturn> list, int first, int? after, int? before, int totalCount) where TReturn : class
    {
        if (before == null)
        {
            return Range(
                list,
                skip: after.GetValueOrDefault(0),
                take: first,
                totalCount);
        }

        return Range(
            list,
            skip: Math.Max(before.Value - first, 0),
            take: first,
            totalCount);
    }

    static Task<Connection<TReturn>> Last<TReturn>(IQueryable<TReturn> list, int last, int? after, int? before, int count)
        where TReturn : class
    {
        if (before == null)
        {
            // last after
            return Range(
                list,
                skip: after.GetValueOrDefault(-1)+1,
                take: last,
                count,
                true);
        }

        // last before
        return Range(
            list,
            skip: Math.Max(before.Value - last, 0),
            take: last,
            count,
            true);
    }

    static async Task<Connection<TReturn>> Range<TReturn>(IQueryable<TReturn> list, int skip, int take, int totalCount, bool reverse=false)
        where TReturn : class
    {
        var page = list.Skip(skip).Take(take);

        var result = await page
            .ToListAsync()
            .ConfigureAwait(false);
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
            TotalCount = totalCount,
            Edges = edges,
            PageInfo = new PageInfo
            {
                HasNextPage = totalCount > take + skip,
                HasPreviousPage = skip > 0 && take < totalCount,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(totalCount - 1, take - 1 + skip).ToString(),
            }
        };
    }
}