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
        if (!SelectExpressionBuilder.TryBuild<Target>(projection, keyNames, out var expression))
        {
            throw new InvalidOperationException("Failed to build the select expression");
        }

        return expression;
    }

    [Test]
    public async Task FieldOrderDoesNotChangeTheExpression()
    {
        // The same fields requested in a different order must produce the same tree, otherwise EF
        // caches a separate compiled query for identical work.
        var forward = Build("Alpha", "Beta", "Gamma");
        var reversed = Build("Gamma", "Beta", "Alpha");
        var shuffled = Build("Beta", "Gamma", "Alpha");

        await Assert.That(reversed.ToString()).IsEqualTo(forward.ToString());
        await Assert.That(shuffled.ToString()).IsEqualTo(forward.ToString());
    }

    [Test]
    public async Task DifferentFieldSetsStillDiffer() =>
        await Assert.That(Build("Alpha", "Gamma").ToString()).IsNotEqualTo(Build("Alpha", "Beta").ToString());
}
