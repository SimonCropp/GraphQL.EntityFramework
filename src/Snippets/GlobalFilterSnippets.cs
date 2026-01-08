// ReSharper disable UnusedParameter.Local

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    public static void Add(ServiceCollection services)
    {
        #region filter-all-fields

        var filters = new Filters<MyDbContext>();
        filters.For<MyEntity>().Add(
            projection: _ => new
            {
                _.Property,
                _.Quantity,
                _.IsActive
            },
            filter: (userContext, dbContext, userPrincipal, projected) =>
                projected.Property != "Ignore" &&
                projected.Quantity > 0 &&
                projected.IsActive);
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public DbSet<Category> Categories { get; set; } = null!;
    }

    #region projection-filter

    public class ChildEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string? Property { get; set; }
    }

    #endregion

    public static void AddProjectionFilter(ServiceCollection services)
    {
        #region projection-filter

        var filters = new Filters<MyDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => new
            {
                _.ParentId
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
        public Guid CategoryId { get; set; }
    }

    #endregion

    public static void AddValueTypeProjections(ServiceCollection services)
    {
        #region value-type-projections

        var filters = new Filters<MyDbContext>();

        // Filter using a string property
        filters.For<Product>().Add(
            projection: _ => _.Name!,
            filter: (_, _, _, name) => name != "Discontinued");

        // Filter using an int property
        filters.For<Product>().Add(
            projection: _ => _.Stock,
            filter: (_, _, _, stock) => stock > 0);

        // Filter using a bool property
        filters.For<Product>().Add(
            projection: _ => _.IsActive,
            filter: (_, _, _, isActive) => isActive);

        // Filter using a DateTime property
        filters.For<Product>().Add(
            projection: _ => _.CreatedAt,
            filter: (_, _, _, createdAt) => createdAt >= new DateTime(2024, 1, 1));

        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    #region nullable-value-type-projections

    public class Order
    {
        public Guid Id { get; set; }
        public int? Quantity { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ShippedAt { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public Customer Customer { get; set; } = null!;
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }
        public bool IsVisible { get; set; }
    }

    #endregion

    public static void AddNullableValueTypeProjections(ServiceCollection services)
    {
        #region nullable-value-type-projections

        var filters = new Filters<MyDbContext>();

        // Filter nullable int - only include if has value and meets condition
        filters.For<Order>().Add(
            projection: _ => _.Quantity,
            filter: (_, _, _, quantity) => quantity is > 0);

        // Filter nullable bool - only include if explicitly approved
        filters.For<Order>().Add(
            projection: _ => _.IsApproved,
            filter: (_, _, _, isApproved) => isApproved == true);

        // Filter nullable DateTime - only include if shipped after date
        filters.For<Order>().Add(
            projection: _ => _.ShippedAt,
            filter: (_, _, _, shippedAt) =>
                shippedAt.HasValue && shippedAt.Value >= new DateTime(2024, 1, 1));

        // Filter nullable string - only include non-null values
        filters.For<Order>().Add(
            projection: _ => _.Notes,
            filter: (_, _, _, notes) => notes != null);

        // Filter nullable int - only include null values
        filters.For<Order>().Add(
            projection: _ => _.Quantity,
            filter: (_, _, _, quantity) => !quantity.HasValue);

        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public static void AddAsyncFilter(ServiceCollection services)
    {
        #region async-filter

        var filters = new Filters<MyDbContext>();
        filters.For<Product>().Add(
            projection: _ => _.CategoryId,
            filter: async (_, dbContext, _, categoryId) =>
            {
                var category = await dbContext.Categories.FindAsync(categoryId);
                return category?.IsVisible == true;
            });
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public static void AddNavigationPropertyFilter(ServiceCollection services)
    {
        #region navigation-property-filter

        var filters = new Filters<MyDbContext>();
        filters.For<Order>().Add(
            projection: o => new { o.TotalAmount, o.Customer.IsActive },
            filter: (_, _, _, x) => x.TotalAmount >= 100 && x.IsActive);
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }
}
