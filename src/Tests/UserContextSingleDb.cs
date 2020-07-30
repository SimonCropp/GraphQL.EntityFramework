using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class UserContextSingleDb<TDbContext> :
    Dictionary<string, object>
    where TDbContext : DbContext
{
    public UserContextSingleDb(TDbContext context)
    {
        DbContext = context;
    }

    public readonly TDbContext DbContext;
}