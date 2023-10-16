public class UserContextSingleDb<TDbContext>(TDbContext context) :
    Dictionary<string, object?>
    where TDbContext : DbContext
{
    public readonly TDbContext DbContext = context;
}