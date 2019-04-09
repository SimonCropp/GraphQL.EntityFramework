
public class ContextFactory
{
    public MyDbContext BuildContext() => DbContextBuilder.BuildDbContext();
}