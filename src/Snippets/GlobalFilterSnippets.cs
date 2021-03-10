using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
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

        Filters filters = new();
        filters.Add<MyEntity>(
            (userContext, item) => item.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: x => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
    }
}