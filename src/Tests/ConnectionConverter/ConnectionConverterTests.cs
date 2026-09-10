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
    //no page size, the whole set
    [InlineData(null, null, null, null)]

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

    //first before
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //first after before
    [InlineData(2, 2, null, 8)]
    [InlineData(20, 2, null, 5)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]
    [InlineData(null, 3, 2, null)]

    //last after before
    [InlineData(null, 2, 2, 8)]
    [InlineData(null, 2, 20, 5)]

    //first and last
    [InlineData(5, 1, 2, null)]

    //last larger than the available range
    [InlineData(null, null, 20, null)]
    [InlineData(null, null, 5, 2)]

    //empty window
    [InlineData(2, null, null, 0)]
    [InlineData(2, 9, null, null)]
    [InlineData(2, 5, null, 3)]
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
    //no page size, the whole set
    [InlineData(null, null, null, null)]

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

    //first before
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //first after before
    [InlineData(2, 2, null, 8)]
    [InlineData(20, 2, null, 5)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]
    [InlineData(null, 3, 2, null)]

    //last after before
    [InlineData(null, 2, 2, 8)]
    [InlineData(null, 2, 20, 5)]

    //first and last
    [InlineData(5, 1, 2, null)]

    //last larger than the available range
    [InlineData(null, null, 20, null)]
    [InlineData(null, null, 5, 2)]

    //empty window
    [InlineData(2, null, null, 0)]
    [InlineData(2, 9, null, null)]
    [InlineData(2, 5, null, 3)]
    public Task List(int? first, int? after, int? last, int? before)
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        return Verify(connection)
            .UseParameters(first, after, last, before);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("")]
    [InlineData("1.5")]
    [InlineData("9999999999999999999")]
    public void Malformed_cursor_reports_bad_input(string cursor)
    {
        // cursors are client supplied, so a malformed one must not surface as a raw FormatException
        var exception = Assert.Throws<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, cursor, null, null));
        Assert.Contains("cursor must be an integer", exception.Message);
    }

    [Fact]
    public void Negative_after_cursor_is_rejected()
    {
        // a negative cursor previously parsed happily and reached the database as a negative OFFSET
        var exception = Assert.Throws<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, "-5", null, null));
        Assert.Contains("cursor cannot be negative", exception.Message);
    }

    [Fact]
    public void Negative_before_cursor_is_rejected()
    {
        var exception = Assert.Throws<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, null, null, "-5"));
        Assert.Contains("cursor cannot be negative", exception.Message);
    }

    [Fact]
    public void Large_first_does_not_overflow_page_info()
    {
        // take + skip overflowed to negative, so HasNextPage reported true on a fully covered page
        var connection = ConnectionConverter.ApplyConnectionContext(list, int.MaxValue, after: 0, last: null, before: null);

        Assert.False(connection.PageInfo!.HasNextPage);
        Assert.Equal("9", connection.PageInfo.EndCursor);
    }

    // The in memory path reversed the page for `last` while numbering the cursors ascending, so
    // cursor 8 sat on item j. Edges keep their order whichever end the page was taken from.
    [Fact]
    public void Last_keeps_edge_order_and_cursors()
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: null, after: null, last: 2, before: null);
        Assert.Equal(["i", "j"], connection.Edges!.Select(_ => _.Node));
        Assert.Equal(["8", "9"], connection.Edges!.Select(_ => _.Cursor));
        Assert.Equal("8", connection.PageInfo!.StartCursor);
        Assert.Equal("9", connection.PageInfo.EndCursor);
    }

    [Fact]
    public void First_before_takes_from_the_start_of_the_window()
    {
        // first: 2 before: 5 is a, b, not the two items before the cursor
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 2, after: null, last: null, before: 5);
        Assert.Equal(["a", "b"], connection.Edges!.Select(_ => _.Node));
        Assert.True(connection.PageInfo!.HasNextPage);
        Assert.False(connection.PageInfo.HasPreviousPage);
    }

    [Fact]
    public void Last_after_takes_from_the_end_of_the_window()
    {
        // last: 2 after: 3 is i, j, not the two items after the cursor
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: null, after: 3, last: 2, before: null);
        Assert.Equal(["i", "j"], connection.Edges!.Select(_ => _.Node));
        Assert.False(connection.PageInfo!.HasNextPage);
        Assert.True(connection.PageInfo.HasPreviousPage);
    }

    [Fact]
    public void Empty_page_has_null_cursors()
    {
        // an empty connection reported startCursor 0 and endCursor -1
        var connection = ConnectionConverter.ApplyConnectionContext(new List<string>(), first: 2, after: null, last: null, before: null);
        Assert.Empty(connection.Edges!);
        Assert.Null(connection.PageInfo!.StartCursor);
        Assert.Null(connection.PageInfo.EndCursor);
        Assert.False(connection.PageInfo.HasNextPage);
        Assert.False(connection.PageInfo.HasPreviousPage);
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
