public class FieldBuilderExtensionsTests
{
    class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    class TestDbContext(DbContextOptions options) : DbContext(options);

    // Identity projections are rejected before the service is touched, so no real service is required
    static readonly IEfGraphQLService<TestDbContext> graphQlService = null!;

    [Test]
    public async Task Resolve_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            field.Resolve(
                graphQlService,
                projection: _ => _,
                resolve: _ => _.Projection.Id));

        await Assert.That(exception.Message).Contains("Identity projection");
        await Assert.That(exception.Message).Contains("_ => _");
    }

    [Test]
    public async Task Resolve_ThrowsArgumentException_WhenIdentityProjectionUsedWithDiscardVariable()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            field.Resolve(
                graphQlService,
                projection: _ => _,
                resolve: _ => _.Projection.Id));

        await Assert.That(exception.Message).Contains("Identity projection");
    }

    [Test]
    public async Task ResolveAsync_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            field.ResolveAsync(
                graphQlService,
                projection: _ => _,
                resolve: _ => Task.FromResult(_.Projection.Id)));

        await Assert.That(exception.Message).Contains("Identity projection");
    }

    [Test]
    public async Task ResolveList_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            field.ResolveList(
                graphQlService,
                projection: _ => _,
                resolve: _ => [_.Projection.Id]));

        await Assert.That(exception.Message).Contains("Identity projection");
    }

    [Test]
    public async Task ResolveListAsync_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            field.ResolveListAsync(
                graphQlService,
                projection: _ => _,
                resolve: _ => Task.FromResult<IEnumerable<int>>([_.Projection.Id])));

        await Assert.That(exception.Message).Contains("Identity projection");
    }

    [Test]
    public async Task FieldBuilderEx_Resolve_ThrowsException()
    {
        // Arrange
        var fieldType = new FieldType { Name = "test", Type = typeof(StringGraphType) };
        var builder = new FieldBuilderEx<object, string>(fieldType);

        // Assert - Calling Resolve should throw
        var exception = Assert.ThrowsExactly<Exception>(() =>
            builder.Resolve(_ => "test"));

        await Assert.That(exception.Message).Contains("resolve has already been configured");
    }

    enum TestEnum
    {
        Value1,
        Value2
    }
}
