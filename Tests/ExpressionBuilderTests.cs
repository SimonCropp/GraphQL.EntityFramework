using System;
using System.Collections.Generic;
using System.Linq;
using EfCoreGraphQL;
using Xunit;

public class ExpressionBuilderTests
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

        var result = list.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<Target>("Member.Length", "==", 2))
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

        var resultFromString = list.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<TargetWithNullable>("Field", "==", "10"))
            .Single();
        Assert.Equal(10, resultFromString.Field);
        var result = list.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<TargetWithNullable>("Field", "==", 10))
            .Single();
        Assert.Equal(10, result.Field);
        var nullResult = list.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<TargetWithNullable>("Field", "==", null))
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

        var result = list.AsQueryable()
            .Where(ExpressionBuilder.BuildInPredicate<TargetForIn>("Member", new List<string> {"Value2"}))
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

        var result = list.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<TargetWithField>("Field", "==", "Target2"))
            .Single();
        Assert.Equal("Target2", result.Field);
    }

    public class TargetWithField
    {
        public string Field;
    }

    [Theory]
    [InlineData("Name", "==", "Person 1", "Person 1")]
    [InlineData("Name", "!=", "Person 2", "Person 1")]
    [InlineData("Name", "Contains", "son 2", "Person 2")]
    [InlineData("Name", "StartsWith", "Person 2", "Person 2")]
    [InlineData("Name", "EndsWith", "son 2", "Person 2")]
    [InlineData("Age", "==", "13", "Person 2")]
    [InlineData("Age", "==", 13, "Person 2")]
    [InlineData("Age", ">", 12, "Person 2")]
    [InlineData("Age", "!=", 12, "Person 2")]
    [InlineData("Age", ">=", 13, "Person 2")]
    [InlineData("Age", "<", 13, "Person 1")]
    [InlineData("Age", "<=", 12, "Person 1")]
    public void Combos(string name, string expression, object value, string expectedName)
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

        var result = people.AsQueryable()
            .Where(ExpressionBuilder.BuildPredicate<Person>(name, expression, value))
            .Single();
        Assert.Equal(expectedName, result.Name);
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [Theory]
    [InlineData(typeof(int), "12", 12)]
    [InlineData(typeof(int?), null, null)]
    public void ConvertStringToType(Type type, string value, object expected)
    {
        var result = ExpressionBuilder.ConvertStringToType(value, type);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertStringToGuid()
    {
        var guid = Guid.NewGuid();
        var value = guid.ToString();
        var result = ExpressionBuilder.ConvertStringToType(value, typeof(Guid));
        Assert.Equal(guid, result);
    }

    [Fact]
    public void ConvertStringToDatetime()
    {
        var dateTime = DateTime.Now.Date;
        var value = dateTime.ToString();
        var result = ExpressionBuilder.ConvertStringToType(value, typeof(DateTime));
        Assert.Equal(dateTime, result);
    }
}