using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedParameter.Local

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public string? Property { get; set; }
    }

    #endregion

    public void Add(ServiceCollection services)
    {

        #region add-filter

        IModel BuildModel()
        {
            var builder = new DbContextOptionsBuilder<MyDbContext>();
            using var dbContext = new MyDbContext(builder.Options);
            return dbContext.Model;
        }

        var model = BuildModel();

        var filters = new Filters();
        filters.Add<MyEntity>(
            (userContext, item) => item.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            model,
            resolveFilters: x => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {

        }
    }
}