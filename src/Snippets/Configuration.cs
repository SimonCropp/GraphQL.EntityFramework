using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

class Configuration
{
    void RegisterInContainerServiceCollectionUsage(IServiceCollection serviceCollection)
    {
        #region RegisterInContainerServiceCollectionUsage

        EfGraphQLConventions.RegisterInContainer(serviceCollection, new MyDbContext());

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public MyDbContext()
        {
        }
    }
}