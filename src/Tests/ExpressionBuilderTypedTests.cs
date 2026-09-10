// The WhereExpression tree the where graph produces: typed values and collection quantifiers
public class ExpressionBuilderTypedTests
{
    static List<ExpressionBuilderTests.Company> Companies() =>
    [
        new()
        {
            Name = "AllYoung",
            Employees =
            [
                new()
                {
                    Name = "A",
                    Age = 20
                },
                new()
                {
                    Name = "B",
                    Age = 25
                }
            ]
        },
        new()
        {
            Name = "Mixed",
            Employees =
            [
                new()
                {
                    Name = "C",
                    Age = 20
                },
                new()
                {
                    Name = "D",
                    Age = 50
                }
            ]
        },
        new()
        {
            Name = "AllOld",
            Employees =
            [
                new()
                {
                    Name = "E",
                    Age = 50
                }
            ]
        },
        new()
        {
            Name = "Empty",
            Employees = []
        }
    ];

    static WhereExpression Collection(Quantifier quantifier, bool negate = false) =>
        new()
        {
            Path = "Employees",
            Quantifier = quantifier,
            Negate = negate,
            GroupedExpressions =
            [
                new()
                {
                    Path = "Age",
                    Comparison = Comparison.LessThan,
                    Value = [30]
                },
                new()
                {
                    Path = "Name",
                    Comparison = Comparison.NotEqual,
                    Value = ["X"]
                }
            ]
        };

    [Theory]
    [InlineData(Quantifier.Any, false, "AllYoung,Mixed")]
    [InlineData(Quantifier.All, false, "AllYoung,Empty")]
    [InlineData(Quantifier.None, false, "AllOld,Empty")]
    [InlineData(Quantifier.Any, true, "AllOld,Empty")]
    public void Quantifiers(Quantifier quantifier, bool negate, string expected)
    {
        var predicate = ExpressionBuilder<ExpressionBuilderTests.Company>.BuildPredicate(Collection(quantifier, negate));
        var names = Companies()
            .AsQueryable()
            .Where(predicate)
            .Select(_ => _.Name);
        Assert.Equal(expected, string.Join(",", names));
    }

    [Fact]
    public void Quantifier_without_conditions_is_has_items()
    {
        var predicate = ExpressionBuilder<ExpressionBuilderTests.Company>.BuildPredicate(
            new WhereExpression
            {
                Path = "Employees",
                Quantifier = Quantifier.Any,
                GroupedExpressions = []
            });
        var names = Companies()
            .AsQueryable()
            .Where(predicate)
            .Select(_ => _.Name);
        Assert.Equal("AllYoung,Mixed,AllOld", string.Join(",", names));
    }

    [Fact]
    public void Typed_values()
    {
        var guid = new Guid("00000000-0000-0000-0000-000000000001");
        var date = new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var targets = new List<Target>
        {
            new()
            {
                Id = guid,
                Count = 10,
                Day = DayOfWeek.Monday,
                When = date,
                Nullable = 5
            },
            new()
            {
                Id = Guid.NewGuid(),
                Count = 11,
                Day = DayOfWeek.Tuesday,
                When = date.AddDays(1),
                Nullable = null
            }
        };

        WhereExpression[] wheres =
        [
            new()
            {
                Path = "Id",
                Value = [guid]
            },
            new()
            {
                Path = "Count",
                Comparison = Comparison.In,
                Value = [10, 12]
            },
            new()
            {
                Path = "Day",
                Value = [DayOfWeek.Monday]
            },
            new()
            {
                Path = "When",
                Comparison = Comparison.LessThan,
                Value = [date.AddHours(1)]
            },
            new()
            {
                Path = "Nullable",
                Comparison = Comparison.In,
                Value = [5, null]
            }
        ];

        var predicate = ExpressionBuilder<Target>.BuildPredicate(wheres);
        var result = targets
            .AsQueryable()
            .Where(predicate)
            .Single();
        Assert.Equal(guid, result.Id);
    }

    // A value of a compatible type, such as an int for a long, is converted
    [Fact]
    public void Compatible_value_is_converted()
    {
        var targets = new List<Target>
        {
            new()
            {
                Big = 10
            },
            new()
            {
                Big = 11
            }
        };

        var predicate = ExpressionBuilder<Target>.BuildPredicate(
            new WhereExpression
            {
                Path = "Big",
                Value = [10]
            });
        var result = targets
            .AsQueryable()
            .Where(predicate)
            .Single();
        Assert.Equal(10, result.Big);
    }

    [Fact]
    public void Duplicate_typed_values_throw()
    {
        var exception = Assert.Throws<Exception>(() =>
            ExpressionBuilder<Target>.BuildPredicate(
                new WhereExpression
                {
                    Path = "Count",
                    Comparison = Comparison.In,
                    Value = [10, 10]
                }));
        Assert.Contains("Duplicates", exception.InnerException!.Message);
    }

    public class Target
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
        public long Big { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime When { get; set; }
        public int? Nullable { get; set; }
    }
}
