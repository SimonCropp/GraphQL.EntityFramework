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

    [Test]
    public async Task Field_returns_EfFieldBuilder_through_fluent_chain()
    {
        var graphType = BuildGraphType();

        var builder = graphType.Field<int>("test")
            .Description("description")
            .DeprecationReason("reason")
            .Argument<IntGraphType>("argument")
            .Configure(_ => _.Metadata["key"] = "value");

        await Assert.That(builder).IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, int>>();
        await Assert.That(builder.FieldType.Description).IsEqualTo("description");
        await Assert.That(builder.FieldType.DeprecationReason).IsEqualTo("reason");
        await Assert.That(builder.FieldType.Metadata["key"]).IsEqualTo("value");
        await Assert.That(builder.FieldType.Arguments?.Find("argument")).IsNotNull();
    }

    [Test]
    public async Task Field_overloads_return_EfFieldBuilder()
    {
        var graphType = BuildGraphType();

        await Assert.That(graphType.Field<IntGraphType, int>("a"))
            .IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, int>>();
        await Assert.That(graphType.Field<IntGraphType>("b"))
            .IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, object>>();
        await Assert.That(graphType.Field("c", typeof(IntGraphType)))
            .IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, object>>();
        await Assert.That(graphType.Field("d", new IntGraphType()))
            .IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, object>>();
        await Assert.That(graphType.Field("e", _ => _.Id)).IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, int>>();
        await Assert.That(graphType.Field(_ => _.Id)).IsTypeOf<EfFieldBuilder<TestDbContext, TestEntity, int>>();
    }

    [Test]
    public async Task Resolve_with_projection_after_fluent_chain()
    {
        var graphType = BuildGraphType();

        var builder = graphType.Field<int>("test")
            .Description("description")
            .Resolve(
                projection: _ => _.Parent,
                resolve: _ => _.Projection?.Id ?? 0)
            .DeprecationReason("reason");

        await Assert.That(builder.FieldType.Metadata.ContainsKey("_EF_Projection")).IsTrue();
        await Assert.That(builder.FieldType.Resolver).IsNotNull();
        await Assert.That(builder.FieldType.DeprecationReason).IsEqualTo("reason");
    }

    [Test]
    public async Task Resolve_rejects_identity_projection()
    {
        var graphType = BuildGraphType();

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            graphType.Field<int>("test")
                .Resolve(
                    projection: _ => _,
                    resolve: _ => _.Projection.Id));

        await Assert.That(exception.Message).Contains("Identity projection");
    }

    [Test]
    public async Task ResolveAsync_rejects_identity_projection()
    {
        var graphType = BuildGraphType();

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            graphType.Field<int>("test")
                .ResolveAsync(
                    projection: _ => _,
                    resolve: _ => Task.FromResult(_.Projection.Id)));

        await Assert.That(exception.Message).Contains("Identity projection");
    }
}
