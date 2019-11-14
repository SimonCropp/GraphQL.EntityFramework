
public class ContextFactory
{
    public SampleDbContext BuildContext() => DbContextBuilder.BuildDbContext();
}