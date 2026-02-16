public class TestingFilterSnippets :
    LocalDbTestBase<TestingFilterSnippets.TestDbContext>
{
    public class TestDbContext(DbContextOptions<TestDbContext> options) :
        DbContext(options)
    {
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
    }

    public class Order
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string? Status { get; set; }
        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    static readonly Guid ownerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    static readonly Guid orderId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    static readonly Guid categoryId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    static TestingFilterSnippets() =>
        Initialize(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                dbContext.Categories.Add(new()
                {
                    Id = categoryId,
                    Name = "TestCategory"
                });
                dbContext.Orders.Add(new()
                {
                    Id = orderId,
                    OwnerId = ownerId,
                    Status = "Active",
                    CategoryId = categoryId
                });
                await dbContext.SaveChangesAsync();
            },
            constructInstance: builder => new(builder.Options));

    static ClaimsPrincipal CreateUser(Guid userId) =>
        new(new ClaimsIdentity([new("UserId", userId.ToString())]));

    #region testing-register-filters

    static void RegisterFilters(Filters<TestDbContext> filters) =>
        filters.For<Order>().Add(
            projection: _ => new
            {
                _.OwnerId,
                CategoryName = _.Category != null ? _.Category.Name : null
            },
            filter: (_, _, user, projection) =>
                projection.OwnerId == Guid.Parse(user!.FindFirst("UserId")!.Value));

    #endregion

    [Fact]
    public async Task ShouldInclude_WhenUserOwnsEntity_ReturnsTrue()
    {
        #region testing-should-include

        // Build filters
        var filters = new Filters<TestDbContext>();
        // call the same method used to register filters at startup
        RegisterFilters(filters);

        var entityId = orderId;

        // Load entity from DB
        var entity = (await ActData.FindAsync(typeof(Order), entityId))!;

        // Load reference navigations needed by filter projections
        var entry = ActData.Entry(entity);
        foreach (var nav in entry.Navigations)
        {
            if (nav is { Metadata: INavigation { IsCollection: false }, IsLoaded: false })
            {
                await nav.LoadAsync();
            }
        }

        var userContext = new Dictionary<string, object?>();

        var result = await filters.ShouldInclude(
            userContext,
            ActData,
            CreateUser(ownerId),
            entity);
        // result is true if all filters allow access, false if any filter excludes the entity

        #endregion

        Assert.True(result);
    }

    [Fact]
    public async Task ShouldInclude_WhenUserDoesNotOwnEntity_ReturnsFalse()
    {
        var filters = new Filters<TestDbContext>();
        RegisterFilters(filters);

        var entity = (await ActData.FindAsync(typeof(Order), orderId))!;
        var entry = ActData.Entry(entity);
        foreach (var nav in entry.Navigations)
        {
            if (nav is { Metadata: INavigation { IsCollection: false }, IsLoaded: false })
            {
                await nav.LoadAsync();
            }
        }

        var otherUserId = Guid.Parse("00000000-0000-0000-0000-999999999999");
        var result = await filters.ShouldInclude(
            new Dictionary<string, object?>(),
            ActData,
            CreateUser(otherUserId),
            entity);

        Assert.False(result);
    }

    [Fact]
    public async Task ShouldInclude_WhenNoFiltersRegistered_ReturnsTrue()
    {
        var filters = new Filters<TestDbContext>();
        // no filters registered

        var entity = (await ActData.FindAsync(typeof(Order), orderId))!;

        var result = await filters.ShouldInclude(
            new Dictionary<string, object?>(),
            ActData,
            CreateUser(Guid.Empty),
            entity);

        Assert.True(result);
    }
}
