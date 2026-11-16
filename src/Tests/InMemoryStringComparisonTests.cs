// The in memory path, used for navigation collections whose arguments could not be applied in
// the query, evaluated string comparisons ordinally, so the same where matched different rows
// than it did on a root field, where the database collation applies. It now ignores case, and
// like, which threw outside a query, matches the pattern in memory.
public class InMemoryStringComparisonTests
{
    [Test]
    [Arguments(Comparison.Equal, "VALUE", "Value")]
    [Arguments(Comparison.StartsWith, "VAL", "Value")]
    [Arguments(Comparison.EndsWith, "LUE", "Value")]
    [Arguments(Comparison.Contains, "ALU", "Value")]
    [Arguments(Comparison.In, "VALUE", "Value")]
    [Arguments(Comparison.Like, "%ALU%", "Value")]
    [Arguments(Comparison.Like, "V_LUE", "Value")]
    [Arguments(Comparison.Like, "[A-Z]alue", "Value")]
    public async Task Matches_ignoring_case(Comparison comparison, string value, string property)
    {
        var result = Apply(comparison, value, new ParentEntity {Property = property}, new ParentEntity {Property = "Other"});

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) [property], CollectionOrdering.Matching);
    }

    [Test]
    [Arguments(Comparison.Like, "Val", "Value")]
    [Arguments(Comparison.Like, "V_LUE", "Vaalue")]
    [Arguments(Comparison.Like, "%.%", "Value")]
    public async Task Like_does_not_match(Comparison comparison, string value, string property)
    {
        var result = Apply(comparison, value, new ParentEntity {Property = property});

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Negated_equal_ignores_case()
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

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) ["Other"], CollectionOrdering.Matching);
    }

    // The where has no null check on a non nullable string, and the member can still be null in
    // memory, for example when it was not loaded
    [Test]
    [Arguments(Comparison.StartsWith)]
    [Arguments(Comparison.EndsWith)]
    [Arguments(Comparison.Contains)]
    public async Task Null_in_non_nullable_string_does_not_match(Comparison comparison)
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

        await Assert.That(result.Select(_ => _.Member)).IsEquivalentTo(["A"], CollectionOrdering.Matching);
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
