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

    public static  Task<Connection<TReturn>> ApplyConnectionContext<TReturn>(IQueryable<TReturn> list, int? first, int? after, int? last, int? before)
        where TReturn : class
    {
        if (last == null)
        {
            return First(list, first.GetValueOrDefault(0), after, before);
        }
        return Last(list, last.Value, after, before);
    }

    static Task<Connection<TReturn>> First<TReturn>(IQueryable<TReturn> list, int first, int? after, int? before) where TReturn : class
    {
        if (before == null)
        {
            return FirstAfter(list, first, after.GetValueOrDefault(0));
        }
        return FirstBefore(list, first, before.Value);
    }

    static async Task<Connection<TReturn>> FirstAfter<TReturn>(IQueryable<TReturn> list, int first, int after) where TReturn : class
    {
        var totalCount = await list.CountAsync().ConfigureAwait(false);
        var take = first;
        var page = list.Skip(after).Take(take);

        var result = await page
            .ToListAsync()
            .ConfigureAwait(false);
        var edges = result
            .Select((item, index) =>
                new Edge<TReturn>
                {
                    Cursor = (index + after).ToString(),
                    Node = item
                })
            .ToList();
        return new Connection<TReturn>
        {
            TotalCount = totalCount,
            PageInfo = new PageInfo
            {
                HasNextPage = totalCount > take + after,
                HasPreviousPage = after > 0 && take<totalCount,
                StartCursor = after.ToString(),
                EndCursor = Math.Min(totalCount-1, take - 1 + after).ToString(),
            },
            Edges = edges
        };
    }
    static  Task<Connection<TReturn>> FirstBefore<TReturn>(IQueryable<TReturn> list, int first, int before) where TReturn : class
    {
        return null;
    }

    static Task<Connection<TReturn>> Last<TReturn>(IQueryable<TReturn> list, int last, int? after, int? before)
        where TReturn : class
    {
        if (before == null)
        {
            return LastAfter(list, last, after.GetValueOrDefault(0));
        }
        return LastBefore(list, last, before.Value);
    }

    static  Task<Connection<TReturn>> LastBefore<TReturn>(IQueryable<TReturn> list, int last, int before)
        where TReturn : class
    {
        return null;
    }
    static  Task<Connection<TReturn>> LastAfter<TReturn>(IQueryable<TReturn> list, int last, int after)
        where TReturn : class
    {
        return null;
    }
}