using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext ResolveDbContext<TSource>(ResolveFieldContext<TSource> context);

        IQueryable<TItem> AddIncludes<TItem, TSource>(IQueryable<TItem> query, ResolveFieldContext<TSource> context)
            where TItem : class;
    }
}