
public class ContextFactory
{
    public GraphQlEfSampleDbContext BuildContext() => DbContextBuilder.BuildDbContext();
}