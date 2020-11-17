using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

static class PaginationConverter
{
    public static Task<Pagination<TItem>> ApplyPaginationContext<TSource, TItem>(
        this IQueryable<TItem> list,
        int? page,
        int? row,
        IResolveFieldContext<TSource> context,
        CancellationToken cancellation,
        Filters filters)
        where TItem : class
    {
        return ApplyPaginationContext(list, page, row, context, filters, cancellation);
    }

    public static async Task<Pagination<TItem>> ApplyPaginationContext<TSource, TItem>(
        IQueryable<TItem> list,
        int? page,
        int? row,
        IResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation = default)
        where TItem : class
    {
        var count = await list.CountAsync(cancellation);
        cancellation.ThrowIfCancellationRequested(); 
        return await Skip(list, page, row, count, context, filters, cancellation);

    }

    static Task<Pagination<TItem>> Skip<TSource, TItem>(
        IQueryable<TItem> list,
        int? page,
        int? row,
        int count,
        IResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation)
        where TItem : class
    {
        var p = page ?? 1;
        var r = row ?? 50;
        var skip = (p - 1) * r;
        var take = r;
        return Range(list, skip, take, count, p, context, filters, cancellation);
    }


    static async Task<Pagination<TItem>> Range<TSource, TItem>(
        IQueryable<TItem> list,
        int skip,
        int take,
        int count,
        int page,
        IResolveFieldContext<TSource> context,
        Filters filters,
        CancellationToken cancellation)
        where TItem : class
    {
        var target = list.Skip(skip).Take(take);
        IEnumerable<TItem> result = await target.ToListAsync(cancellation);
        result = await filters.ApplyFilter(result, context.UserContext);

        cancellation.ThrowIfCancellationRequested();
        return Build(take, count, result, page);
    }

    static Pagination<T> Build<T>(int take, int count, IEnumerable<T> result, int page)
    {

        var totalPages = Convert.ToInt32(Math.Ceiling((count + 0.0) / take));
        return new Pagination<T>(
            result.ToList(),  new PaginationMetaData()
            {
                Total = count,
                TotalPages = totalPages,
                Row = take,
                Page = page
            });
    }

}