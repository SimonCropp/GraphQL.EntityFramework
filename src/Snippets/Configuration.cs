class Configuration
{
    #region ModelBuilder
    static class ModelBuilder
    {
        public static IModel GetInstance()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("Fake");
            using var context = new MyDbContext(builder.Options);
            return context.Model;
        }
    }
    #endregion

    static void RegisterInContainerExplicit(IServiceCollection serviceCollection) =>
    #region RegisterInContainer
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            serviceCollection,
            model: ModelBuilder.GetInstance());
    #endregion


    public class MyDbContext(DbContextOptions options) :
        DbContext(options);
}