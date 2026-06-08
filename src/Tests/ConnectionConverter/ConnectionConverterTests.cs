public class ConnectionConverterTests
{
    static ConnectionConverterTests() =>
        sqlInstance = new(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                dbContext.AddRange(list.Select(_ => new Entity {Property = _}));
                await dbContext.SaveChangesAsync();
            },
            constructInstance: builder => new(builder.Options));

    static List<string> list = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];

    static SqlInstance<MyContext> sqlInstance;

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]
    public async Task Queryable(int? first, int? after, int? last, int? before)
    {
        var fieldContext = new ResolveFieldContext<string>();
        await using var database = await sqlInstance.Build(databaseSuffix: $"{first.GetValueOrDefault(0)}{after.GetValueOrDefault(0)}{last.GetValueOrDefault(0)}{before.GetValueOrDefault(0)}");
        var entities = database.Context.Entities;
        var connection = await ConnectionConverter.ApplyConnectionContext<MyContext, string, Entity>(entities.OrderBy(x=>x.Property), first, after, last, before, fieldContext, new(), Cancel.None,database.Context);
        await Verify(connection.Items!.OrderBy(_ => _!.Property))
            .UseParameters(first, after, last, before);
    }

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]

    public Task List(int? first, int? after, int? last, int? before)
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        return Verify(connection).UseParameters(first, after, last, before);
    }

    [Fact]
    public void List_after_is_an_exclusive_cursor()
    {
        // 'after' is an exclusive cursor: results start strictly after the given index,
        // matching the IQueryable path and the Relay connection spec.
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 2, after: 1, last: null, before: null);
        var items = connection.Items!
            .Select(_ => _!)
            .ToList();
        Assert.Equal(new[] {"c", "d"}, items);
    }

    [Fact]
    public void List_HasPreviousPage_true_when_page_size_covers_remaining_items()
    {
        // first(10) covers all remaining items, but after(0) skips the first item,
        // so a previous page exists. Previously the 'take < count' guard hid this.
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 10, after: 0, last: null, before: null);
        Assert.True(connection.PageInfo!.HasPreviousPage);
        var first = connection.Items!
            .Select(_ => _!)
            .First();
        Assert.Equal("b", first);
    }

    public class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Property { get; set; }
    }

    public class MyContext(DbContextOptions options) :
        DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Entity>();
    }
}