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
            projection: entity => new()
            {
                Property = entity.Property
            },
            filter: (userContext, dbContext, userPrincipal, projected) =>
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
            projection: child => new()
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

    #region value-type-projections

    public class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    public static void AddValueTypeProjections(ServiceCollection services)
    {
        #region value-type-projections

        var filters = new Filters<MyDbContext>();

        // Filter using a string property
        filters.Add<Product, string>(
            projection: entity => entity.Name!,
            filter: (_, _, _, name) => name != "Discontinued");

        // Filter using an int property
        filters.Add<Product, int>(
            projection: entity => entity.Stock,
            filter: (_, _, _, stock) => stock > 0);

        // Filter using a bool property
        filters.Add<Product, bool>(
            projection: entity => entity.IsActive,
            filter: (_, _, _, isActive) => isActive);

        // Filter using a DateTime property
        filters.Add<Product, DateTime>(
            projection: entity => entity.CreatedAt,
            filter: (_, _, _, createdAt) => createdAt >= new DateTime(2024, 1, 1));

        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }
}
