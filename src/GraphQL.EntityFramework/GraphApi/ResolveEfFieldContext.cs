using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public class ResolveEfFieldContext<TDbContext, TSource> :
        ResolveFieldContext<TSource>
        where TDbContext : DbContext
    {
        public TDbContext DbContext { get; set; } = null!;
        public Filters Filters { get; set; } = null!;
    }
}