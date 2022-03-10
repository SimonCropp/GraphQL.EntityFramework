public class UserContext : Dictionary<string, object>
{
    public UserContext(SampleDbContext context) =>
        DbContext = context;

    public readonly SampleDbContext DbContext;
}