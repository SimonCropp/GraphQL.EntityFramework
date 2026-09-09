public class EfFieldBuilderTests
{
    class TestEntity
    {
        public int Id { get; set; }
        public TestEntity? Parent { get; set; }
    }

    class TestDbContext(DbContextOptions options) : DbContext(options);

    class TestGraphType(IEfGraphQLService<TestDbContext> graphQlService) :
        EfObjectGraphType<TestDbContext, TestEntity>(graphQlService);

    // The service is not touched when defining fields, so none is required
    static TestGraphType BuildGraphType() => new(null!);

    [Fact]
    public void Field_returns_EfFieldBuilder_through_fluent_chain()
    {
        var graphType = BuildGraphType();

        var builder = graphType.Field<int>("test")
            .Description("description")
            .DeprecationReason("reason")
            .Argument<IntGraphType>("argument")
            .Configure(_ => _.Metadata["key"] = "value");

        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, int>>(builder);
        Assert.Equal("description", builder.FieldType.Description);
        Assert.Equal("reason", builder.FieldType.DeprecationReason);
        Assert.Equal("value", builder.FieldType.Metadata["key"]);
        Assert.NotNull(builder.FieldType.Arguments?.Find("argument"));
    }

    [Fact]
    public void Field_overloads_return_EfFieldBuilder()
    {
        var graphType = BuildGraphType();

        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, int>>(graphType.Field<IntGraphType, int>("a"));
        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, object>>(graphType.Field<IntGraphType>("b"));
        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, object>>(graphType.Field("c", typeof(IntGraphType)));
        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, object>>(graphType.Field("d", new IntGraphType()));
        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, int>>(graphType.Field("e", _ => _.Id));
        Assert.IsType<EfFieldBuilder<TestDbContext, TestEntity, int>>(graphType.Field(_ => _.Id));
    }

    [Fact]
    public void Resolve_with_projection_after_fluent_chain()
    {
        var graphType = BuildGraphType();

        var builder = graphType.Field<int>("test")
            .Description("description")
            .Resolve(
                projection: _ => _.Parent,
                resolve: _ => _.Projection?.Id ?? 0)
            .DeprecationReason("reason");

        Assert.True(builder.FieldType.Metadata.ContainsKey("_EF_Projection"));
        Assert.NotNull(builder.FieldType.Resolver);
        Assert.Equal("reason", builder.FieldType.DeprecationReason);
    }

    [Fact]
    public void Resolve_rejects_identity_projection()
    {
        var graphType = BuildGraphType();

        var exception = Assert.Throws<ArgumentException>(() =>
            graphType.Field<int>("test")
                .Resolve(
                    projection: _ => _,
                    resolve: _ => _.Projection.Id));

        Assert.Contains("Identity projection", exception.Message);
    }

    [Fact]
    public void ResolveAsync_rejects_identity_projection()
    {
        var graphType = BuildGraphType();

        var exception = Assert.Throws<ArgumentException>(() =>
            graphType.Field<int>("test")
                .ResolveAsync(
                    projection: _ => _,
                    resolve: _ => Task.FromResult(_.Projection.Id)));

        Assert.Contains("Identity projection", exception.Message);
    }
}
