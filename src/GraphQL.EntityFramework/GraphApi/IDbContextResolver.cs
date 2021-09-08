using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework.GraphApi
{
    public interface IDbContextResolver<TDbContext> where TDbContext : DbContext
    {
        TDbContext ResolveDbContext(object userContext);
    }
}