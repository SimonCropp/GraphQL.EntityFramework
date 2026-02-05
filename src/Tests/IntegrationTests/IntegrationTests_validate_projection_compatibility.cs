public partial class IntegrationTests
{
    [Fact]
    public void Querying_abstract_type_returns_false()
    {
        var projection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "Property" },
            null,
            null,
            null);

        var keyNames = new Dictionary<Type, List<string>>();

        var result = SelectExpressionBuilder.TryBuild<BaseEntity>(projection, keyNames, out var expression);

        Assert.False(result);
        Assert.Null(expression);
    }

    [Fact]
    public void Filter_with_identity_projection_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: _ => _,
                filter: (_, _, _, entity) => entity.Property == "Test"));

        Assert.Null(exception);
    }

    [Fact]
    public void Filter_with_explicit_projection_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: e => new { e.Id, e.Property },
                filter: (_, _, _, proj) => proj.Property == "Test"));

        Assert.Null(exception);
    }
}
