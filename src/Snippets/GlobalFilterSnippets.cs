using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public string Property { get; set; }
    }

    #endregion

    public void Add(ServiceCollection services, IModel model)
    {
        #region add-filter

        var filters = new GlobalFilters();
        filters.Add<MyEntity>(
            (userContext, item) => { return item.Property != "Ignore"; });
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(services, model, filters);

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
    }
}