public class ExpressionBuilderTests
{
    public class Target
    {
        public string? Member { get; set; }
    }

    [Test]
    public async Task Nested()
    {
        var list = new List<Target>
        {
            new()
            {
                Member = "a"
            },
            new()
            {
                Member = "bb"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<Target>.BuildPredicate("Member.Length", Comparison.Equal, ["2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo("bb");
    }

    [Test]
    public async Task Nested_notEqual()
    {
        var list = new List<Target>
        {
            new()
            {
                Member = "a"
            },
            new()
            {
                Member = "bb"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<Target>.BuildPredicate("Member.Length", Comparison.NotEqual, ["2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo("a");
    }

    [Test]
    public async Task Nullable_requiring_parse()
    {
        var guid = new Guid("00000000-0000-0000-0000-000000000001");
        var list = new List<TargetWithNullableRequiringParse>
        {
            new()
            {
                Field = null
            },
            new()
            {
                Field = guid
            }
        };

        var resultFromString = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.Equal, [guid.ToString()]))
            .Single();

        await Assert.That(resultFromString.Field).IsEqualTo(guid);

        var nullResult = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.Equal, null))
            .Single();

        await Assert.That(nullResult.Field).IsNull();
    }

    [Test]
    public async Task Nullable_requiring_parse_notEqual()
    {
        var guid = new Guid("00000000-0000-0000-0000-000000000001");
        var list = new List<TargetWithNullableRequiringParse>
        {
            new()
            {
                Field = null
            },
            new()
            {
                Field = guid
            }
        };

        var resultFromString = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.NotEqual, [guid.ToString()]))
            .Single();

        await Assert.That(resultFromString.Field).IsNull();

        var nullResult = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.NotEqual, null))
            .Single();

        await Assert.That(nullResult.Field).IsEqualTo(guid);
    }

    [Test]
    public async Task Nullable_requiring_parse_In()
    {
        var guid = new Guid("00000000-0000-0000-0000-000000000001");
        var list = new List<TargetWithNullableRequiringParse>
        {
            new()
            {
                Field = null
            },
            new()
            {
                Field = guid
            }
        };

        var resultFromNull = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, [null]))
            .Single();

        await Assert.That(resultFromNull.Field).IsNull();

        var resultWithGuid = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, [guid.ToString()]))
            .Single();

        await Assert.That(resultWithGuid.Field).IsEqualTo(guid);

        var resultGuidAndNull = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullableRequiringParse>.BuildPredicate("Field", Comparison.In, [guid.ToString(), null]))
            .Select(parse => parse.Field)
            .ToList();

        await Assert.That(resultGuidAndNull).IsEquivalentTo((Guid?[]) [null, guid], CollectionOrdering.Matching);
    }

    public class TargetWithNullableRequiringParse
    {
        public Guid? Field;
    }

    [Test]
    public async Task Nullable()
    {
        var list = new List<TargetWithNullable>
        {
            new()
            {
                Field = null
            },
            new()
            {
                Field = 10
            }
        };

        var resultFromString = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, ["10"]))
            .Single();
        await Assert.That(resultFromString.Field).IsEqualTo(10);
        var nullResult = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.Equal, null))
            .Single();
        await Assert.That(nullResult.Field).IsNull();
    }

    [Test]
    public async Task Nullable_notEqual()
    {
        var list = new List<TargetWithNullable>
        {
            new()
            {
                Field = null
            },
            new()
            {
                Field = 10
            }
        };

        var resultFromString = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.NotEqual, ["10"]))
            .Single();
        await Assert.That(resultFromString.Field).IsNull();
        var nullResult = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithNullable>.BuildPredicate("Field", Comparison.NotEqual, null))
            .Single();
        await Assert.That(nullResult.Field).IsEqualTo(10);
    }

    public class TargetWithNullable
    {
        public int? Field;
    }

    public class TargetChildForPropertyNestedExpression
    {
        public string? Member;
    }

    [Test]
    public async Task InList()
    {
        var list = new List<TargetForIn>
        {
            new()
            {
                Member = "Value1"
            },
            new()
            {
                Member = "Value2"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForIn>.BuildPredicate("Member", Comparison.In, ["Value2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo("Value2");
    }

    [Test]
    public async Task NotInList()
    {
        var list = new List<TargetForIn>
        {
            new()
            {
                Member = "Value1"
            },
            new()
            {
                Member = "Value2"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForIn>.BuildPredicate("Member", Comparison.NotIn, ["Value2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo("Value1");
    }

    public class TargetForIn
    {
        public string? Member;
    }

    [Test]
    public async Task InIntList()
    {
        var list = new List<TargetForInInt>
        {
            new()
            {
                Member = 1
            },
            new()
            {
                Member = 2
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForInInt>.BuildPredicate("Member", Comparison.In, ["2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo(2);
    }

    [Test]
    public async Task NotInIntList()
    {
        var list = new List<TargetForInInt>
        {
            new()
            {
                Member = 1
            },
            new()
            {
                Member = 2
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForInInt>.BuildPredicate("Member", Comparison.NotIn, ["2"]))
            .Single();
        await Assert.That(result.Member).IsEqualTo(1);
    }

    public class TargetForInInt
    {
        public int Member;
    }

    [Test]
    public async Task InGuidList()
    {
        var list = new List<TargetForInGuid>
        {
            new()
            {
                Member = new("00000000-0000-0000-0000-000000000001")
            },
            new()
            {
                Member = new("00000000-0000-0000-0000-000000000002")
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForInGuid>.BuildPredicate("Member", Comparison.In, ["00000000-0000-0000-0000-000000000002"]))
            .Single();
        await Assert.That(result).IsSameReferenceAs(list[1]);
    }

    [Test]
    public async Task NotInGuidList()
    {
        var list = new List<TargetForInGuid>
        {
            new()
            {
                Member = new("00000000-0000-0000-0000-000000000001")
            },
            new()
            {
                Member = new("00000000-0000-0000-0000-000000000002")
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetForInGuid>.BuildPredicate("Member", Comparison.NotIn, ["00000000-0000-0000-0000-000000000002"]))
            .Single();
        await Assert.That(result).IsSameReferenceAs(list[0]);
    }

    public class TargetForInGuid
    {
        public Guid Member;
    }

    [Test]
    public async Task Field()
    {
        var list = new List<TargetWithField>
        {
            new()
            {
                Field = "Target1"
            },
            new()
            {
                Field = "Target2"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithField>.BuildPredicate("Field", Comparison.Equal, ["Target2"]))
            .Single();
        await Assert.That(result.Field).IsEqualTo("Target2");
    }

    [Test]
    public async Task Field_notEqual()
    {
        var list = new List<TargetWithField>
        {
            new()
            {
                Field = "Target1"
            },
            new()
            {
                Field = "Target2"
            }
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithField>.BuildPredicate("Field", Comparison.NotEqual, ["Target2"]))
            .Single();
        await Assert.That(result.Field).IsEqualTo("Target1");
    }

    [Test]
    public async Task Contains()
    {
        var list = new List<TargetWithField>
        {
            new()
            {
                Field = "Target1"
            },
            new()
            {
                Field = "Target2"
            },
            new()
        };

        var result = list
            .AsQueryable()
            .Where(ExpressionBuilder<TargetWithField>.BuildPredicate("Field", Comparison.Contains, ["Target2"]))
            .Single();
        await Assert.That(result.Field).IsEqualTo("Target2");
    }

    public class TargetWithField
    {
        public string? Field;
    }

    // A non nullable string is a required property in the EF model. EF removes a null check on
    // one, and Verify.EntityFramework throws for it.
    [Test]
    [Arguments(Comparison.StartsWith)]
    [Arguments(Comparison.EndsWith)]
    [Arguments(Comparison.Contains)]
    public async Task Non_nullable_string_has_no_null_check(Comparison comparison)
    {
        var predicate = ExpressionBuilder<TargetWithNonNullable>.BuildPredicate("Member", comparison, ["a"]);
        await Assert.That(predicate.ToString()).DoesNotContain("!= null");
    }

    [Test]
    [Arguments(Comparison.StartsWith)]
    [Arguments(Comparison.EndsWith)]
    [Arguments(Comparison.Contains)]
    public async Task Nullable_string_has_null_check(Comparison comparison)
    {
        var predicate = ExpressionBuilder<Target>.BuildPredicate("Member", comparison, ["a"]);
        await Assert.That(predicate.ToString()).Contains("(_.Member != null)");
    }

    // The navigation can be null, and then so is the member read through it
    [Test]
    public async Task Non_nullable_string_through_navigation_has_null_check()
    {
        var predicate = ExpressionBuilder<TargetWithNonNullable>.BuildPredicate("Parent.Member", Comparison.StartsWith, ["a"]);
        await Assert.That(predicate.ToString()).Contains("(_.Parent.Member != null)");
    }

    [Test]
    public async Task Non_nullable_string_in_list_has_no_null_check()
    {
        var predicate = ExpressionBuilder<TargetWithNonNullable>.BuildPredicate("Children[Member]", Comparison.StartsWith, ["a"]);
        await Assert.That(predicate.ToString()).DoesNotContain("!= null");
    }

    // notEqual and notIn are already negated, so negating them again unwraps them
    [Test]
    [Arguments(Comparison.NotEqual)]
    [Arguments(Comparison.NotIn)]
    public async Task Negated_negative_comparison_is_not_negated_twice(Comparison comparison)
    {
        var list = new List<Target>
        {
            new()
            {
                Member = "a"
            },
            new()
            {
                Member = "b"
            }
        };

        var predicate = ExpressionBuilder<Target>.BuildPredicate("Member", comparison, ["a"], true);

        await Assert.That(predicate.ToString()).DoesNotContain("Not(");
        var result = list
            .AsQueryable()
            .Where(predicate)
            .Single();
        await Assert.That(result.Member).IsEqualTo("a");
    }

    [Test]
    public async Task Negated_none_is_not_negated_twice()
    {
        var predicate = ExpressionBuilder<TargetWithNonNullable>.BuildPredicate(
            new WhereExpression
            {
                Path = "Children",
                Quantifier = Quantifier.None,
                Negate = true,
                GroupedExpressions =
                [
                    new()
                    {
                        Path = "Member",
                        Comparison = Comparison.Equal,
                        Value = ["a"]
                    }
                ]
            });

        await Assert.That(predicate.ToString()).DoesNotContain("Not(");
    }

    public class TargetWithNonNullable
    {
        public string Member { get; set; } = "";
        public TargetWithNonNullable? Parent { get; set; }
        public List<TargetWithNonNullable> Children { get; set; } = [];
    }

    [Test]
    [Arguments("Name", Comparison.Equal, "Person 1", "Person 1")]
    [Arguments("Name", Comparison.Equal, "Person 2", "Person 1", true)]
    [Arguments("Name", Comparison.Contains, "son 2", "Person 2")]
    [Arguments("Name", Comparison.StartsWith, "Person 2", "Person 2")]
    [Arguments("Name", Comparison.EndsWith, "son 2", "Person 2")]
    [Arguments("Age", Comparison.Equal, "13", "Person 2")]
    [Arguments("Age", Comparison.GreaterThan, "12", "Person 2")]
    [Arguments("Age", Comparison.Equal, "12", "Person 2", true)]
    [Arguments("Age", Comparison.GreaterThanOrEqual, "13", "Person 2")]
    [Arguments("Age", Comparison.LessThan, "13", "Person 1")]
    [Arguments("Age", Comparison.LessThanOrEqual, "12", "Person 1")]
    [Arguments("DateOfBirth", Comparison.Equal, "2001-10-10T10:10:10+00:00", "Person 1")]
    [Arguments("DateOfBirth.Day", Comparison.Equal, "11", "Person 2")]
    public async Task Combos(string name, Comparison expression, string value, string expectedName, bool negate = false)
    {
        var people = new List<Person>
        {
            new()
            {
                Name = "Person 1",
                Age = 12,
                DateOfBirth = new(2001, 10, 10, 10, 10, 10, DateTimeKind.Utc)
            },
            new()
            {
                Name = "Person 2",
                Age = 13,
                DateOfBirth = new(2000, 10, 11, 10, 10, 10, DateTimeKind.Utc)
            },
        };

        var result = people
            .AsQueryable()
            .Where(ExpressionBuilder<Person>.BuildPredicate(name, expression, [value], negate))
            .Single();
        await Assert.That(result.Name).IsEqualTo(expectedName);
    }

    [Test]
    [Arguments("Employees.Count", Comparison.Equal, "3", "Company 1")]
    [Arguments("Employees.Count", Comparison.LessThan, "3", "Company 2")]
    [Arguments("Employees.Count", Comparison.LessThanOrEqual, "2", "Company 2")]
    [Arguments("Employees.Count", Comparison.GreaterThan, "2", "Company 1")]
    [Arguments("Employees.Count", Comparison.GreaterThanOrEqual, "3", "Company 1")]
    [Arguments("Employees[Name]", Comparison.Equal, "Person 1", "Company 1")]
    [Arguments("Employees[Name]", Comparison.Equal, "Person 3", "Company 1", true)]
    [Arguments("Employees[Name]", Comparison.Contains, "son 2", "Company 1")]
    [Arguments("Employees[Name]", Comparison.StartsWith, "Person 2", "Company 1")]
    [Arguments("Employees[Name]", Comparison.EndsWith, "son 2", "Company 1")]
    [Arguments("Employees[Age]", Comparison.Equal, "12", "Company 1")]
    [Arguments("Employees[Age]", Comparison.GreaterThan, "12", "Company 2")]
    [Arguments("Employees[Age]", Comparison.Equal, "12", "Company 2", true)]
    [Arguments("Employees[Age]", Comparison.GreaterThanOrEqual, "31", "Company 2")]
    [Arguments("Employees[Age]", Comparison.LessThan, "13", "Company 1")]
    [Arguments("Employees[Age]", Comparison.LessThanOrEqual, "12", "Company 1")]
    [Arguments("Employees[DateOfBirth]", Comparison.Equal, "2001-10-10T10:10:10+00:00", "Company 1")]
    [Arguments("Employees[DateOfBirth.Day]", Comparison.Equal, "11", "Company 2")]
    [Arguments("Employees[Company.Employees[Name]]", Comparison.Contains, "son 2", "Company 1")]
    public async Task ListMemberQueryCombos(string name, Comparison expression, string value, string expectedName, bool negate = false)
    {
        var companies = new List<Company>
        {
            new()
            {
                Name = "Company 1",
                Employees =
                [
                    new()
                    {
                        Name = "Person 1",
                        Age = 12,
                        DateOfBirth = new(1999, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                    },
                    new()
                    {
                        Name = "Person 2",
                        Age = 12,
                        DateOfBirth = new(2001, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                    },
                    new()
                    {
                        Name = "Person 4",
                        Age = 11,
                        DateOfBirth = new(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                    }
                ]
            },

            new()
            {
                Name = "Company 2",
                Employees =
                [
                    new()
                    {
                        Name = "Person 3",
                        Age = 34,
                        DateOfBirth = new(1977, 10, 11, 10, 10, 10, DateTimeKind.Utc)
                    },
                    new()
                    {
                        Name = "Person 3",
                        Age = 31,
                        DateOfBirth = new(1980, 10, 11, 10, 10, 10, DateTimeKind.Utc)
                    },
                ]
            }
        };

        foreach (var company in companies)
        {
            foreach (var employee in company.Employees)
            {
                employee.Company = company;
            }
        }

        var result = companies
            .AsQueryable()
            .Where(ExpressionBuilder<Company>.BuildPredicate(name, expression, [value], negate))
            .Single();
        await Assert.That(result.Name).IsEqualTo(expectedName);
    }

    public class Company
    {
        public string? Name { get; set; }
        public IList<Person> Employees { get; set; } = null!;
    }

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Company? Company { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}