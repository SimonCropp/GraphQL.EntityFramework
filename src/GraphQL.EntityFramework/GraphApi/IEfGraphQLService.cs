using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context);
    }
}