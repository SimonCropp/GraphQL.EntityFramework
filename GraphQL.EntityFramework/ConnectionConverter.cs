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
        // do count prior to any paging. skip/take etc
        var totalCount = await list.CountAsync().ConfigureAwait(false);

        if (last == null)
        {
            return await First(list, first.GetValueOrDefault(0), after, before, totalCount).ConfigureAwait(false);
        }
        return await Last(list, last.Value, after, before, totalCount).ConfigureAwait(false);

    }

    static async Task<Connection<TReturn>> First<TReturn>(IQueryable<TReturn> list, int first, int? after, int? before, int totalCount) where TReturn : class
    {
        return await FirstAfter(list, first, after, totalCount);
    }

    private static async Task<Connection<TReturn>> FirstAfter<TReturn>(IQueryable<TReturn> list, int first, int? after, int totalCount) where TReturn : class
    {
        var take = first;
        var page = list.Take(take);
        var skip = after.GetValueOrDefault(0);

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
        return new Connection<TReturn>
        {
            TotalCount = totalCount,
            PageInfo = new PageInfo
            {
                HasNextPage = totalCount > take + skip,
                HasPreviousPage = skip > 0,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(totalCount, take - 1 + skip).ToString(),
            },
            Edges = edges
        };
    }
    private static async Task<Connection<TReturn>> FirstBefore<TReturn>(IQueryable<TReturn> list, int first, int before, int totalCount) where TReturn : class
    {
        var take = first;
        var page = list.Take(take);
        var skip = after.GetValueOrDefault(0);

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
        return new Connection<TReturn>
        {
            TotalCount = totalCount,
            PageInfo = new PageInfo
            {
                HasNextPage = totalCount > take + skip,
                HasPreviousPage = skip > 0,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(totalCount, take - 1 + skip).ToString(),
            },
            Edges = edges
        };
    }

    static async Task<Connection<TReturn>> Last<TReturn>(IQueryable<TReturn> list, int last, int? after, int? before, int totalCount)
        where TReturn : class
    {
        var take = last;
        var page = list.Take(take);
        var skip = after.GetValueOrDefault(0);

        var beforeValue = before.Value;
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
        return new Connection<TReturn>
        {
            TotalCount = totalCount,
            PageInfo = new PageInfo
            {
                HasNextPage = totalCount > take + skip,
                HasPreviousPage = skip > 0,
                StartCursor = skip.ToString(),
                EndCursor = Math.Min(totalCount, take - 1 + skip).ToString(),
            },
            Edges = edges
        };
    }
}