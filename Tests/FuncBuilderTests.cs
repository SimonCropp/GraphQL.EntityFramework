using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using Xunit;

public class FuncBuilderTests
{
    public class Target
    {
        public string Member { get; set; }
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

        var result = list
            .Where(FuncBuilder<Target>.BuildPredicate("Member.Length", Comparison.Equal, "2"))
            .Single();
        Assert.Equal("bb", result.Member);
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

        var resultFromString = list
            .Where(FuncBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, "10"))
            .Single();
        Assert.Equal(10, resultFromString.Field);
        var nullResult = list
            .Where(FuncBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, null))
            .Single();
        Assert.Null(nullResult.Field);
    }

    public class TargetWithNullable
    {
        public int? Field;
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

        var result = list
            .Where(FuncBuilder<TargetForIn>.BuildIn("Member", new List<string> {"Value2"}))
            .Single();
        Assert.Contains("Value2", result.Member);
    }

    public class TargetForIn
    {
        public string Member;
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

        var result = list
            .Where(FuncBuilder<TargetWithField>.BuildPredicate("Field", Comparison.Equal, "Target2"))
            .Single();
        Assert.Equal("Target2", result.Field);
    }

    public class TargetWithField
    {
        public string Field;
    }

    [Theory]
    [InlineData("Name", Comparison.Equal, "Person 1", "Person 1")]
    [InlineData("Name", Comparison.NotEqual, "Person 2", "Person 1")]
    [InlineData("Name", Comparison.Contains, "son 2", "Person 2")]
    [InlineData("Name", Comparison.StartsWith, "Person 2", "Person 2")]
    [InlineData("Name", Comparison.EndsWith, "son 2", "Person 2")]
    [InlineData("Age", Comparison.Equal, "13", "Person 2")]
    [InlineData("Age", Comparison.GreaterThan, "12", "Person 2")]
    [InlineData("Age", Comparison.NotEqual, "12", "Person 2")]
    [InlineData("Age", Comparison.GreaterThanOrEqual,"13", "Person 2")]
    [InlineData("Age", Comparison.LessThan, "13", "Person 1")]
    [InlineData("Age", Comparison.LessThanOrEqual, "12", "Person 1")]
    public void Combos(string name, Comparison expression, string value, string expectedName)
    {
        var people = new List<Person>
        {
            new Person
            {
                Name = "Person 1",
                Age = 12
            },
            new Person
            {
                Name = "Person 2",
                Age = 13
            }
        };

        var result = people
            .Where(FuncBuilder<Person>.BuildPredicate(name, expression, value))
            .Single();
        Assert.Equal(expectedName, result.Name);
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}