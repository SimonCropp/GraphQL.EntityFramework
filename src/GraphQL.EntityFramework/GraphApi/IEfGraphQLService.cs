using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
    }
}