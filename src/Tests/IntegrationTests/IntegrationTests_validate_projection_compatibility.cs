public partial class IntegrationTests
{
    [Test]
    public async Task Querying_abstract_type_returns_false()
    {
        var projection = new FieldProjectionInfo(
            [with(StringComparer.OrdinalIgnoreCase), "Property"],
            null,
            null,
            null);

        var keyNames = new Dictionary<Type, List<string>>();

        var result = SelectExpressionBuilder.TryBuild<BaseEntity>(projection, keyNames, out var expression);

        await Assert.That(result).IsFalse();
        await Assert.That(expression).IsNull();
    }

    [Test]
    public async Task Filter_with_identity_projection_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        await Assert.That(() =>
                filters.For<DerivedChildEntity>().Add(
                    projection: _ => _,
                    filter: (_, _, _, entity) => entity.Property == "Test"))
            .ThrowsNothing();
    }

    [Test]
    public async Task Filter_with_explicit_projection_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        await Assert.That(() =>
                filters.For<DerivedChildEntity>().Add(
                    projection: _ => new { _.Id, _.Property },
                    filter: (_, _, _, proj) => proj.Property == "Test"))
            .ThrowsNothing();
    }
}
