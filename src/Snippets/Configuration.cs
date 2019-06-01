using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

class Configuration
{
    void RegisterInContainerServiceCollectionUsage(IServiceCollection serviceCollection)
    {
        #region RegisterInContainerServiceCollectionUsage

        EfGraphQLConventions.RegisterInContainer<MyDbContext>(serviceCollection, MyDbContext.DataModel);

        #endregion
    }

    #region DbContextWithModel

    public class MyDbContext :
        DbContext
    {
        static MyDbContext()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("fake");
            using (var context = new MyDbContext(builder.Options))
            {
                DataModel = context.Model;
            }
        }

        public static readonly IModel DataModel;

        public MyDbContext(DbContextOptions options) :
            base(options)
        {
        }
    }

    #endregion
}