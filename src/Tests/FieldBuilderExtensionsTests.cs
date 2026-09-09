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

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve(
                graphQlService,
                projection: _ => _,
                resolve: _ => _.Projection.Id));

        Assert.Contains("Identity projection", exception.Message);
        Assert.Contains("_ => _", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenIdentityProjectionUsedWithDiscardVariable()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve(
                graphQlService,
                projection: _ => _,
                resolve: _ => _.Projection.Id));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void ResolveAsync_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveAsync(
                graphQlService,
                projection: _ => _,
                resolve: _ => Task.FromResult(_.Projection.Id)));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void ResolveList_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveList(
                graphQlService,
                projection: _ => _,
                resolve: _ => [_.Projection.Id]));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void ResolveListAsync_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveListAsync(
                graphQlService,
                projection: _ => _,
                resolve: _ => Task.FromResult<IEnumerable<int>>([_.Projection.Id])));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void FieldBuilderEx_Resolve_ThrowsException()
    {
        // Arrange
        var fieldType = new FieldType { Name = "test", Type = typeof(StringGraphType) };
        var builder = new FieldBuilderEx<object, string>(fieldType);

        // Assert - Calling Resolve should throw
        var exception = Assert.Throws<Exception>(() =>
            builder.Resolve(_ => "test"));

        Assert.Contains("resolve has already been configured", exception.Message);
    }

    enum TestEnum
    {
        Value1,
        Value2
    }
}
