// ReSharper disable UnusedParameter.Local

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
    }

    #endregion

    #region filter-all-fields

    public class MyEntityFilter
    {
        public string? Property { get; set; }
    }

    #endregion

    public static void Add(ServiceCollection services)
    {
        #region filter-all-fields

        var filters = new Filters<MyDbContext>();
        filters.Add<MyEntity, MyEntityFilter>(
            projection: entity => new MyEntityFilter
            {
                Property = entity.Property
            },
            filter: async (userContext, dbContext, userPrincipal, projected) =>
                projected.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext;

    #region projection-filter

    public class ChildEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string? Property { get; set; }
    }

    public class ChildFilterProjection
    {
        public Guid? ParentId { get; set; }
    }

    #endregion

    public static void AddProjectionFilter(ServiceCollection services)
    {
        #region projection-filter

        var filters = new Filters<MyDbContext>();
        filters.Add<ChildEntity, ChildFilterProjection>(
            projection: child => new ChildFilterProjection
            {
                ParentId = child.ParentId
            },
            filter: (userContext, data, userPrincipal, projected) =>
            {
                var allowedParentId = GetAllowedParentId(userContext);
                return projected.ParentId == allowedParentId;
            });
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    static Guid GetAllowedParentId(object userContext) =>
        Guid.Empty;
}