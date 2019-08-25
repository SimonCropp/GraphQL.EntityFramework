using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public delegate TDbContext ResolveDbContext<out TDbContext>(object userContext)
        where TDbContext : DbContext;
}