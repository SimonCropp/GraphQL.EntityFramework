using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using Xunit;
using Xunit.Abstractions;

public class ExpressionBuilderTests :
    XunitApprovalBase
{
    public class Target
    {
        public string? Member { get; set; }
    }

    [Fact]
    public void Nested()
    {
        var list = new List<Target>
        {
            new Target
            {
                Member = "a"
            },
            new Target
            {
                Member = "bb"
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<Target>.BuildPredicate("Member.Length", Comparison.Equal, new[] {"2"}))
            .Single();
        Assert.Equal("bb", result.Member);
    }

    [Fact]
    public void Nullable_requiring_parse()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var list = new List<TargetWithNullableRequiringParse>
        {
            new TargetWithNullableRequiringParse
            {
                Field = null
            },
            new TargetWithNullableRequiringParse
            {
                Field = guid
            }
        };

        var resultFromString = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.Equal, new[] {guid.ToString()}))
            .Single();

        Assert.Equal(guid, resultFromString.Field);

        var nullResult = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.Equal, null))
            .Single();

        Assert.Null(nullResult.Field);
    }


    [Fact]
    public void Nullable_requiring_parse_In()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var list = new List<TargetWithNullableRequiringParse>
        {
            new TargetWithNullableRequiringParse
            {
                Field = null
            },
            new TargetWithNullableRequiringParse
            {
                Field = guid
            }
        };

        var resultFromNull = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, new[] {(string?) null}))
            .Single();

        Assert.Null(resultFromNull.Field);

        var resultWithGuid = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, new[] {guid.ToString()}))
            .Single();

        Assert.Equal(guid, resultWithGuid.Field);

        var resultGuidAndNull = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, new[] {guid.ToString(), null}))
            .Select(parse => parse.Field)
            .ToList();

        Assert.Equal(resultGuidAndNull, new List<Guid?>{null, guid});
    }

    public class TargetWithNullableRequiringParse
    {
        public Guid? Field;
    }

    [Fact]
    public void Nullable()
    {
        var list = new List<TargetWithNullable>
        {
            new TargetWithNullable
            {
                Field = null
            },
            new TargetWithNullable
            {
                Field = 10
            }
        };

        var resultFromString = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, new[] {"10"}))
            .Single();
        Assert.Equal(10, resultFromString.Field);
        var nullResult = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, null))
            .Single();
        Assert.Null(nullResult.Field);
    }

    public class TargetWithNullable
    {
        public int? Field;
    }

    public class TargetChildForPropertyNestedExpression
    {
        public string? Member;
    }

    [Fact]
    public void InList()
    {
        var list = new List<TargetForIn>
        {
            new TargetForIn
            {
                Member = "Value1"
            },
            new TargetForIn
            {
                Member = "Value2"
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForIn>.BuildPredicate("Member", Comparison.In, new[] {"Value2"}))
            .Single();
        Assert.Equal("Value2", result.Member);
    }

    [Fact]
    public void NotInList()
    {
        var list = new List<TargetForIn>
        {
            new TargetForIn
            {
                Member = "Value1"
            },
            new TargetForIn
            {
                Member = "Value2"
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForIn>.BuildPredicate("Member", Comparison.NotIn, new[] { "Value2" }))
            .Single();
        Assert.Equal("Value1", result.Member);
    }

    public class TargetForIn
    {
        public string? Member;
    }

    [Fact]
    public void InIntList()
    {
        var list = new List<TargetForInInt>
        {
            new TargetForInInt
            {
                Member = 1
            },
            new TargetForInInt
            {
                Member = 2
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForInInt>.BuildPredicate("Member", Comparison.In, new[] {"2"}))
            .Single();
        Assert.Equal(2, result.Member);
    }


    [Fact]
    public void NotInIntList()
    {
        var list = new List<TargetForInInt>
        {
            new TargetForInInt
            {
                Member = 1
            },
            new TargetForInInt
            {
                Member = 2
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForInInt>.BuildPredicate("Member", Comparison.NotIn, new[] { "2" }))
            .Single();
        Assert.Equal(1, result.Member);
    }

    public class TargetForInInt
    {
        public int Member;
    }

    [Fact]
    public void InGuidList()
    {
        var list = new List<TargetForInGuid>
        {
            new TargetForInGuid
            {
                Member = Guid.Parse("00000000-0000-0000-0000-000000000001")
            },
            new TargetForInGuid
            {
                Member = Guid.Parse("00000000-0000-0000-0000-000000000002")
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForInGuid>.BuildPredicate("Member", Comparison.In, new[] {"00000000-0000-0000-0000-000000000002"}))
            .Single();
        Assert.Same(list[1], result);
    }

    [Fact]
    public void NotInGuidList()
    {
        var list = new List<TargetForInGuid>
        {
            new TargetForInGuid
            {
                Member = Guid.Parse("00000000-0000-0000-0000-000000000001")
            },
            new TargetForInGuid
            {
                Member = Guid.Parse("00000000-0000-0000-0000-000000000002")
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetForInGuid>.BuildPredicate("Member", Comparison.NotIn, new[] { "00000000-0000-0000-0000-000000000002" }))
            .Single();
        Assert.Same(list[0], result);
    }

    public class TargetForInGuid
    {
        public Guid Member;
    }

    [Fact]
    public void Field()
    {
        var list = new List<TargetWithField>
        {
            new TargetWithField
            {
                Field = "Target1"
            },
            new TargetWithField
            {
                Field = "Target2"
            }
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithField>.BuildPredicate("Field", Comparison.Equal, new[] {"Target2"}))
            .Single();
        Assert.Equal("Target2", result.Field);
    }

    [Fact]
    public void Contains()
    {
        var list = new List<TargetWithField>
        {
            new TargetWithField
            {
                Field = "Target1"
            },
            new TargetWithField
            {
                Field = "Target2"
            },
            new TargetWithField()
        };

        var result = list.AsQueryable()
            .Where(ExpressionBuilder<TargetWithField>.BuildPredicate("Field", Comparison.Contains, new[] {"Target2"}))
            .Single();
        Assert.Equal("Target2", result.Field);
    }

    public class TargetWithField
    {
        public string? Field;
    }

    [Theory]
    [InlineData("Name", Comparison.Equal, "Person 1", "Person 1", null)]
    [InlineData("Name", Comparison.NotEqual, "Person 2", "Person 1", null)]
    [InlineData("Name", Comparison.Contains, "son 2", "Person 2", null)]
    [InlineData("Name", Comparison.StartsWith, "Person 2", "Person 2", null)]
    [InlineData("Name", Comparison.EndsWith, "son 2", "Person 2", null)]
    [InlineData("Name", Comparison.EndsWith, "person 2", "Person 2", StringComparison.OrdinalIgnoreCase)]
    [InlineData("Age", Comparison.Equal, "13", "Person 2", null)]
    [InlineData("Age", Comparison.GreaterThan, "12", "Person 2", null)]
    [InlineData("Age", Comparison.NotEqual, "12", "Person 2", null)]
    [InlineData("Age", Comparison.GreaterThanOrEqual, "13", "Person 2", null)]
    [InlineData("Age", Comparison.LessThan, "13", "Person 1", null)]
    [InlineData("Age", Comparison.LessThanOrEqual, "12", "Person 1", null)]
    [InlineData("DateOfBirth", Comparison.Equal, "2001-10-10T10:10:10+00:00", "Person 1", null)]
    [InlineData("DateOfBirth.Day", Comparison.Equal, "11", "Person 2", null)]
    public void Combos(string name, Comparison expression, string value, string expectedName, StringComparison? stringComparison)
    {
        var people = new List<Person>
        {
            new Person
            {
                Name = "Person 1",
                Age = 12,
                DateOfBirth = new DateTime(2001, 10, 10, 10, 10, 10, DateTimeKind.Utc)
            },
            new Person
            {
                Name = "Person 2",
                Age = 13,
                DateOfBirth = new DateTime(2000, 10, 11, 10, 10, 10, DateTimeKind.Utc)
            },
        };

        var result = people.AsQueryable()
            .Where(ExpressionBuilder<Person>.BuildPredicate(name, expression, new[] {value}, stringComparison))
            .Single();
        Assert.Equal(expectedName, result.Name);
    }

    [Theory]
    [InlineData("Name", Comparison.Equal, "Person 1", "Person 1", null)]
    [InlineData("Name", Comparison.NotEqual, "Person 2", "Person 1", null)]
    [InlineData("Name", Comparison.Contains, "son 2", "Person 2", null)]
    [InlineData("Name", Comparison.StartsWith, "Person 2", "Person 2", null)]
    [InlineData("Name", Comparison.EndsWith, "son 2", "Person 2", null)]
    [InlineData("Name", Comparison.EndsWith, "person 2", "Person 2", StringComparison.OrdinalIgnoreCase)]
    [InlineData("Age", Comparison.Equal, "13", "Person 2", null)]
    [InlineData("Age", Comparison.GreaterThan, "12", "Person 2", null)]
    [InlineData("Age", Comparison.NotEqual, "12", "Person 2", null)]
    [InlineData("Age", Comparison.GreaterThanOrEqual, "13", "Person 2", null)]
    [InlineData("Age", Comparison.LessThan, "13", "Person 1", null)]
    [InlineData("Age", Comparison.LessThanOrEqual, "12", "Person 1", null)]
    [InlineData("DateOfBirth", Comparison.Equal, "2001-10-10T10:10:10+00:00", "Person 1", null)]
    [InlineData("DateOfBirth.Day", Comparison.Equal, "11", "Person 2", null)]
    public void SingleCombos(string name, Comparison expression, string value, string expectedName, StringComparison? stringComparison)
    {
        var people = new List<Person>
        {
            new Person
            {
                Name = "Person 1",
                Age = 12,
                DateOfBirth = new DateTime(2001, 10, 10, 10, 10, 10, DateTimeKind.Utc)
            },
            new Person
            {
                Name = "Person 2",
                Age = 13,
                DateOfBirth = new DateTime(2000, 10, 11, 10, 10, 10, DateTimeKind.Utc)
            },
        };

        var result = people.AsQueryable()
            .Where(ExpressionBuilder<Person>.BuildSinglePredicate(name, expression, value, stringComparison))
            .Single();
        Assert.Equal(expectedName, result.Name);
    }

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public ExpressionBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}