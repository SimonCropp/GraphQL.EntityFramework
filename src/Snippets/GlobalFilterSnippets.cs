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

        var filters = new Filters();
        filters.Add<MyEntity>(
            (userContext, userPrincipal, item) => item.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext;
}