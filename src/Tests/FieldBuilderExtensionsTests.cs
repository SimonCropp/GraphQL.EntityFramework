public class FieldBuilderExtensionsTests
{
    class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    class TestDbContext : DbContext;

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenIdentityProjectionUsed()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, int, TestEntity>(
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
            field.Resolve<TestDbContext, TestEntity, int, TestEntity>(
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
            field.ResolveAsync<TestDbContext, TestEntity, int, TestEntity>(
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
            field.ResolveList<TestDbContext, TestEntity, int, TestEntity>(
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
            field.ResolveListAsync<TestDbContext, TestEntity, int, TestEntity>(
                projection: _ => _,
                resolve: _ => Task.FromResult<IEnumerable<int>>([_.Projection.Id])));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenProjectingToInt()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, int, int>(
                projection: _ => _.Id,
                resolve: _ => _.Projection * 2));

        Assert.Contains("Projection to scalar type", exception.Message);
        Assert.Contains("Int32", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenProjectingToString()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<string>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, string, string>(
                projection: _ => _.Name,
                resolve: _ => _.Projection.ToUpper()));

        Assert.Contains("Projection to scalar type", exception.Message);
        Assert.Contains("String", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenProjectingToBool()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<bool>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, bool, bool>(
                projection: _ => _.Id > 0,
                resolve: _ => _.Projection));

        Assert.Contains("Projection to scalar type", exception.Message);
        Assert.Contains("Boolean", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenProjectingToDateTime()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<DateTime>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, DateTime, DateTime>(
                projection: _ => DateTime.Now,
                resolve: _ => _.Projection));

        Assert.Contains("Projection to scalar type", exception.Message);
        Assert.Contains("DateTime", exception.Message);
    }

    [Fact]
    public void Resolve_ThrowsArgumentException_WhenProjectingToEnum()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<TestEnum>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.Resolve<TestDbContext, TestEntity, TestEnum, TestEnum>(
                projection: _ => TestEnum.Value1,
                resolve: _ => _.Projection));

        Assert.Contains("Projection to scalar type", exception.Message);
        Assert.Contains("TestEnum", exception.Message);
    }

    [Fact]
    public void ResolveAsync_ThrowsArgumentException_WhenProjectingToScalar()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<int>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveAsync<TestDbContext, TestEntity, int, int>(
                projection: _ => _.Id,
                resolve: _ => Task.FromResult(_.Projection * 2)));

        Assert.Contains("Projection to scalar type", exception.Message);
    }

    [Fact]
    public void ResolveList_ThrowsArgumentException_WhenProjectingToScalar()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveList<TestDbContext, TestEntity, int, int>(
                projection: _ => _.Id,
                resolve: _ => [_.Projection]));

        Assert.Contains("Projection to scalar type", exception.Message);
    }

    [Fact]
    public void ResolveListAsync_ThrowsArgumentException_WhenProjectingToScalar()
    {
        var graphType = new ObjectGraphType<TestEntity>();
        var field = graphType.Field<IEnumerable<int>>("test");

        var exception = Assert.Throws<ArgumentException>(() =>
            field.ResolveListAsync<TestDbContext, TestEntity, int, int>(
                projection: _ => _.Id,
                resolve: _ => Task.FromResult<IEnumerable<int>>([_.Projection])));

        Assert.Contains("Projection to scalar type", exception.Message);
    }

    enum TestEnum
    {
        Value1,
        Value2
    }
}
