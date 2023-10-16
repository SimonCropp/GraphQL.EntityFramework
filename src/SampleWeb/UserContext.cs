public class UserContext(SampleDbContext context) :
    Dictionary<string, object>
{
    public readonly SampleDbContext DbContext = context;
}