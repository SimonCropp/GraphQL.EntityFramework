using System;
using System.Linq;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

class Configuration
{
    void RegisterInContainerServiceCollectionUsage(IServiceCollection serviceCollection)
    {
        #region RegisterInContainerServiceCollectionUsage

        EfGraphQLConventions.RegisterInContainer(serviceCollection, MyDataContext.DataModel);

        #endregion
    }

    #region DataContextWithModel

    public class MyDataContext :
        DbContext
    {
        static MyDataContext()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("fake");
            using (var context = new MyDataContext(builder.Options))
            {
                DataModel = context.Model;
            }
        }

        public static readonly IModel DataModel;

        public MyDataContext(DbContextOptions options) :
            base(options)
        {
        }
    }

    #endregion
}