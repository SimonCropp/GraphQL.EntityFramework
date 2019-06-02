using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class ResolveEfFieldContext<TDbContext,TSource> : ResolveFieldContext<TSource>
    {
        public TDbContext DbContext { get; set; }
    }
}