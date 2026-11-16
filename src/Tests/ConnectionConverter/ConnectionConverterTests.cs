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
            constructInstance: builder => new(builder.ThrowOnAntiPatterns().Options));

    static List<string> list = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];

    static SqlInstance<MyContext> sqlInstance;

    [Test]
    //no page size, the whole set
    [Arguments(null, null, null, null)]

    //first after
    [Arguments(1, 0, null, null)]
    [Arguments(2, null, null, null)]
    [Arguments(2, 1, null, null)]
    [Arguments(3, null, null, null)]
    [Arguments(3, 1, null, null)]
    [Arguments(10, null, null, null)]
    [Arguments(10, 1, null, null)]
    [Arguments(11, null, null, null)]
    [Arguments(11, 1, null, null)]

    //first before
    [Arguments(2, null, null, 2)]
    [Arguments(3, null, null, 2)]
    [Arguments(2, null, null, 3)]

    //first after before
    [Arguments(2, 2, null, 8)]
    [Arguments(20, 2, null, 5)]

    //last before
    [Arguments(null, null, 2, null)]
    [Arguments(null, null, 2, 8)]

    //last after
    [Arguments(null, 7, 2, null)]
    [Arguments(null, 3, 2, null)]

    //last after before
    [Arguments(null, 2, 2, 8)]
    [Arguments(null, 2, 20, 5)]

    //first and last
    [Arguments(5, 1, 2, null)]

    //last larger than the available range
    [Arguments(null, null, 20, null)]
    [Arguments(null, null, 5, 2)]

    //empty window
    [Arguments(2, null, null, 0)]
    [Arguments(2, 9, null, null)]
    [Arguments(2, 5, null, 3)]
    public async Task Queryable(int? first, int? after, int? last, int? before)
    {
        var fieldContext = new ResolveFieldContext<string>();
        await using var database = await sqlInstance.Build(databaseSuffix: $"{first}_{after}_{last}_{before}");
        var entities = database.Context.Entities;
        var connection = await ConnectionConverter.ApplyConnectionContext<MyContext, string, Entity>(entities.OrderBy(x=>x.Property), first, after, last, before, fieldContext, new(), Cancel.None,database.Context);
        await Verify(connection.Items!.OrderBy(_ => _!.Property))
            .UseParameters(first, after, last, before);
    }

    [Test]
    //no page size, the whole set
    [Arguments(null, null, null, null)]

    //first after
    [Arguments(1, 0, null, null)]
    [Arguments(2, null, null, null)]
    [Arguments(2, 1, null, null)]
    [Arguments(3, null, null, null)]
    [Arguments(3, 1, null, null)]
    [Arguments(10, null, null, null)]
    [Arguments(10, 1, null, null)]
    [Arguments(11, null, null, null)]
    [Arguments(11, 1, null, null)]

    //first before
    [Arguments(2, null, null, 2)]
    [Arguments(3, null, null, 2)]
    [Arguments(2, null, null, 3)]

    //first after before
    [Arguments(2, 2, null, 8)]
    [Arguments(20, 2, null, 5)]

    //last before
    [Arguments(null, null, 2, null)]
    [Arguments(null, null, 2, 8)]

    //last after
    [Arguments(null, 7, 2, null)]
    [Arguments(null, 3, 2, null)]

    //last after before
    [Arguments(null, 2, 2, 8)]
    [Arguments(null, 2, 20, 5)]

    //first and last
    [Arguments(5, 1, 2, null)]

    //last larger than the available range
    [Arguments(null, null, 20, null)]
    [Arguments(null, null, 5, 2)]

    //empty window
    [Arguments(2, null, null, 0)]
    [Arguments(2, 9, null, null)]
    [Arguments(2, 5, null, 3)]
    public Task List(int? first, int? after, int? last, int? before)
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        return Verify(connection)
            .UseParameters(first, after, last, before);
    }

    [Test]
    [Arguments("not-a-number")]
    [Arguments("")]
    [Arguments("1.5")]
    [Arguments("9999999999999999999")]
    public async Task Malformed_cursor_reports_bad_input(string cursor)
    {
        // cursors are client supplied, so a malformed one must not surface as a raw FormatException
        var exception = Assert.ThrowsExactly<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, cursor, null, null));
        await Assert.That(exception.Message).Contains("cursor must be an integer");
    }

    [Test]
    public async Task Negative_after_cursor_is_rejected()
    {
        // a negative cursor previously parsed happily and reached the database as a negative OFFSET
        var exception = Assert.ThrowsExactly<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, "-5", null, null));
        await Assert.That(exception.Message).Contains("cursor cannot be negative");
    }

    [Test]
    public async Task Negative_before_cursor_is_rejected()
    {
        var exception = Assert.ThrowsExactly<Exception>(
            () => ConnectionConverter.ApplyConnectionContext(list, 2, null, null, "-5"));
        await Assert.That(exception.Message).Contains("cursor cannot be negative");
    }

    [Test]
    public async Task Large_first_does_not_overflow_page_info()
    {
        // take + skip overflowed to negative, so HasNextPage reported true on a fully covered page
        var connection = ConnectionConverter.ApplyConnectionContext(list, int.MaxValue, after: 0, last: null, before: null);

        await Assert.That(connection.PageInfo!.HasNextPage).IsFalse();
        await Assert.That(connection.PageInfo.EndCursor).IsEqualTo("9");
    }

    // The in memory path reversed the page for `last` while numbering the cursors ascending, so
    // cursor 8 sat on item j. Edges keep their order whichever end the page was taken from.
    [Test]
    public async Task Last_keeps_edge_order_and_cursors()
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: null, after: null, last: 2, before: null);
        await Assert.That(connection.Edges!.Select(_ => _.Node))
            .IsEquivalentTo((string?[]) ["i", "j"], CollectionOrdering.Matching);
        await Assert.That(connection.Edges!.Select(_ => _.Cursor))
            .IsEquivalentTo((string?[]) ["8", "9"], CollectionOrdering.Matching);
        await Assert.That(connection.PageInfo!.StartCursor).IsEqualTo("8");
        await Assert.That(connection.PageInfo.EndCursor).IsEqualTo("9");
    }

    [Test]
    public async Task First_before_takes_from_the_start_of_the_window()
    {
        // first: 2 before: 5 is a, b, not the two items before the cursor
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 2, after: null, last: null, before: 5);
        await Assert.That(connection.Edges!.Select(_ => _.Node))
            .IsEquivalentTo((string?[]) ["a", "b"], CollectionOrdering.Matching);
        await Assert.That(connection.PageInfo!.HasNextPage).IsTrue();
        await Assert.That(connection.PageInfo.HasPreviousPage).IsFalse();
    }

    [Test]
    public async Task Last_after_takes_from_the_end_of_the_window()
    {
        // last: 2 after: 3 is i, j, not the two items after the cursor
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: null, after: 3, last: 2, before: null);
        await Assert.That(connection.Edges!.Select(_ => _.Node))
            .IsEquivalentTo((string?[]) ["i", "j"], CollectionOrdering.Matching);
        await Assert.That(connection.PageInfo!.HasNextPage).IsFalse();
        await Assert.That(connection.PageInfo.HasPreviousPage).IsTrue();
    }

    [Test]
    public async Task Empty_page_has_null_cursors()
    {
        // an empty connection reported startCursor 0 and endCursor -1
        var connection = ConnectionConverter.ApplyConnectionContext(new List<string>(), first: 2, after: null, last: null, before: null);
        await Assert.That(connection.Edges!).IsEmpty();
        await Assert.That(connection.PageInfo!.StartCursor).IsNull();
        await Assert.That(connection.PageInfo.EndCursor).IsNull();
        await Assert.That(connection.PageInfo.HasNextPage).IsFalse();
        await Assert.That(connection.PageInfo.HasPreviousPage).IsFalse();
    }

    [Test]
    public async Task List_after_is_an_exclusive_cursor()
    {
        // 'after' is an exclusive cursor: results start strictly after the given index,
        // matching the IQueryable path and the Relay connection spec.
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 2, after: 1, last: null, before: null);
        var items = connection.Items!
            .Select(_ => _!)
            .ToList();
        await Assert.That(items).IsEquivalentTo(new[] {"c", "d"}, CollectionOrdering.Matching);
    }

    [Test]
    public async Task List_HasPreviousPage_true_when_page_size_covers_remaining_items()
    {
        // first(10) covers all remaining items, but after(0) skips the first item,
        // so a previous page exists. Previously the 'take < count' guard hid this.
        var connection = ConnectionConverter.ApplyConnectionContext(list, first: 10, after: 0, last: null, before: null);
        await Assert.That(connection.PageInfo!.HasPreviousPage).IsTrue();
        var first = connection.Items!
            .Select(_ => _!)
            .First();
        await Assert.That(first).IsEqualTo("b");
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
