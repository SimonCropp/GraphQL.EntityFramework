// ReSharper disable UnusedParameter.Local

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public string? Property { get; set; }
    }

    #endregion

    public static void Add(ServiceCollection services)
    {
        #region add-filter

        var filters = new Filters<MyDbContext>();
        filters.Add<MyEntity>(
            (userContext, dbContext, userPrincipal, item) => item.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext;
}