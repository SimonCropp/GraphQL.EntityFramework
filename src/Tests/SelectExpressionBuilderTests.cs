public class SelectExpressionBuilderTests
{
    public class Target
    {
        public int Id { get; set; }
        public string? Alpha { get; set; }
        public string? Beta { get; set; }
        public string? Gamma { get; set; }
    }

    static readonly Dictionary<Type, List<string>> keyNames = new()
    {
        {
            typeof(Target), ["Id"]
        }
    };

    static Expression<Func<Target, Target>> Build(params string[] scalarFields)
    {
        var projection = new FieldProjectionInfo(
            [with(scalarFields, StringComparer.OrdinalIgnoreCase)],
            ["Id"],
            null,
            null);
        Assert.True(SelectExpressionBuilder.TryBuild<Target>(projection, keyNames, out var expression));
        return expression;
    }

    [Fact]
    public void FieldOrderDoesNotChangeTheExpression()
    {
        // The same fields requested in a different order must produce the same tree, otherwise EF
        // caches a separate compiled query for identical work.
        var forward = Build("Alpha", "Beta", "Gamma");
        var reversed = Build("Gamma", "Beta", "Alpha");
        var shuffled = Build("Beta", "Gamma", "Alpha");

        Assert.Equal(forward.ToString(), reversed.ToString());
        Assert.Equal(forward.ToString(), shuffled.ToString());
    }

    [Fact]
    public void DifferentFieldSetsStillDiffer() =>
        Assert.NotEqual(
            Build("Alpha", "Beta").ToString(),
            Build("Alpha", "Gamma").ToString());
}
