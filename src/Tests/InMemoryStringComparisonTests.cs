// The in memory path, used for navigation collections whose arguments could not be applied in
// the query, evaluated string comparisons ordinally, so the same where matched different rows
// than it did on a root field, where the database collation applies. It now ignores case, and
// like, which threw outside a query, matches the pattern in memory.
public class InMemoryStringComparisonTests
{
    [Theory]
    [InlineData(Comparison.Equal, "VALUE", "Value")]
    [InlineData(Comparison.StartsWith, "VAL", "Value")]
    [InlineData(Comparison.EndsWith, "LUE", "Value")]
    [InlineData(Comparison.Contains, "ALU", "Value")]
    [InlineData(Comparison.In, "VALUE", "Value")]
    [InlineData(Comparison.Like, "%ALU%", "Value")]
    [InlineData(Comparison.Like, "V_LUE", "Value")]
    [InlineData(Comparison.Like, "[A-Z]alue", "Value")]
    public void Matches_ignoring_case(Comparison comparison, string value, string property)
    {
        var result = Apply(comparison, value, new ParentEntity {Property = property}, new ParentEntity {Property = "Other"});

        Assert.Equal([property], result.Select(_ => _.Property));
    }

    [Theory]
    [InlineData(Comparison.Like, "Val", "Value")]
    [InlineData(Comparison.Like, "V_LUE", "Vaalue")]
    [InlineData(Comparison.Like, "%.%", "Value")]
    public void Like_does_not_match(Comparison comparison, string value, string property)
    {
        var result = Apply(comparison, value, new ParentEntity {Property = property});

        Assert.Empty(result);
    }

    [Fact]
    public void Negated_equal_ignores_case()
    {
        var context = BuildContext(
            new()
            {
                Path = "Property",
                Comparison = Comparison.Equal,
                Value = ["VALUE"],
                Negate = true
            });

        var result = new List<ParentEntity>
            {
                new() {Property = "Value"},
                new() {Property = "Other"}
            }
            .ApplyGraphQlArguments(true, context, false)
            .ToList();

        Assert.Equal(["Other"], result.Select(_ => _.Property));
    }

    // The where has no null check on a non nullable string, and the member can still be null in
    // memory, for example when it was not loaded
    [Theory]
    [InlineData(Comparison.StartsWith)]
    [InlineData(Comparison.EndsWith)]
    [InlineData(Comparison.Contains)]
    public void Null_in_non_nullable_string_does_not_match(Comparison comparison)
    {
        var context = BuildContext(
            new()
            {
                Path = "Member",
                Comparison = comparison,
                Value = ["a"]
            });

        var result = new List<WithNonNullable>
            {
                new() {Member = null!},
                new() {Member = "A"}
            }
            .ApplyGraphQlArguments(false, context, false)
            .ToList();

        Assert.Equal(["A"], result.Select(_ => _.Member));
    }

    public class WithNonNullable
    {
        public string Member { get; set; } = "";
    }

    static List<ParentEntity> Apply(Comparison comparison, string value, params ParentEntity[] entities)
    {
        var context = BuildContext(
            new()
            {
                Path = "Property",
                Comparison = comparison,
                Value = [value]
            });

        return entities
            .ApplyGraphQlArguments(true, context, false)
            .ToList();
    }

    static ResolveFieldContext BuildContext(WhereExpression where) =>
        new()
        {
            Arguments = new Dictionary<string, ArgumentValue>
            {
                ["where"] = new(where, ArgumentSource.Literal)
            },
            FieldDefinition = new()
            {
                Name = "entities"
            },
            Errors = []
        };
}
