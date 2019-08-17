using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

class Configuration
{
    void RegisterInContainerViaServiceProviderUsage(IServiceCollection serviceCollection)
    {
        #region RegisterInContainerViaServiceProviderUsage
        EfGraphQLConventions.RegisterInContainer(
            serviceCollection,
            userContext => (MyDbContext)userContext);
        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public MyDbContext(DbContextOptions options):
            base(options)
        {
        }
    }
}