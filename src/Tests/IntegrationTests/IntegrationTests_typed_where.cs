using System.Globalization;

public partial class IntegrationTests
{
    [Fact]
    public async Task Where_and_or_not()
    {
        var query =
            """
            {
              parentEntities
              (
                where: {
                  property: {startsWith: "Valu"},
                  or: [
                    {property: {endsWith: "1"}},
                    {property: {endsWith: "3"}}
                  ],
                  not: {property: {equal: "Value3"}}
                },
                orderBy: {property: ascending}
              )
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };
        var entity3 = new ParentEntity
        {
            Property = "Value3"
        };
        var entity4 = new ParentEntity
        {
            Property = "Other1"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4]);
    }

    [Fact]
    public async Task Where_nested_navigation()
    {
        var query =
            """
            {
              childEntities
              (
                where: {parent: {property: {equal: "Parent1"}}},
                orderBy: {property: ascending}
              )
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Parent1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Parent2"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent2
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent1, parent2, child1, child2]);
    }

    [Theory]
    [InlineData("any")]
    [InlineData("all")]
    [InlineData("none")]
    public async Task Where_collection(string quantifier)
    {
        var query =
            $$$$$"""
            {
              parentEntities
              (
                where: {children: {{{{{{quantifier}}}}}: {property: {startsWith: "Match"}}}},
                orderBy: {property: ascending}
              )
              {
                property
              }
            }
            """;

        var allMatch = new ParentEntity
        {
            Property = "AllMatch",
            Children =
            [
                new()
                {
                    Property = "Match1"
                },
                new()
                {
                    Property = "Match2"
                }
            ]
        };
        var someMatch = new ParentEntity
        {
            Property = "SomeMatch",
            Children =
            [
                new()
                {
                    Property = "Match1"
                },
                new()
                {
                    Property = "Other"
                }
            ]
        };
        var noneMatch = new ParentEntity
        {
            Property = "NoneMatch",
            Children =
            [
                new()
                {
                    Property = "Other"
                }
            ]
        };
        var noChildren = new ParentEntity
        {
            Property = "NoChildren"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [allMatch, someMatch, noneMatch, noChildren]);
    }

    [Fact]
    public async Task Where_collection_any_without_conditions()
    {
        var query =
            """
            {
              parentEntities
              (
                where: {children: {any: {}}},
                orderBy: {property: ascending}
              )
              {
                property
              }
            }
            """;

        var withChildren = new ParentEntity
        {
            Property = "WithChildren",
            Children =
            [
                new()
                {
                    Property = "Child"
                }
            ]
        };
        var noChildren = new ParentEntity
        {
            Property = "NoChildren"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [withChildren, noChildren]);
    }

    // The value arrives as a GraphQL Decimal, so the server culture plays no part
    [Fact]
    public async Task Where_typed_value_is_culture_independent()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities (where: {salary: {equal: 10.5}})
              {
                name
                salary
              }
            }
            """;

        var match = new FieldBuilderProjectionEntity
        {
            Name = "Match",
            Salary = 10.5m
        };
        var other = new FieldBuilderProjectionEntity
        {
            Name = "Other",
            Salary = 105m
        };

        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new("de-DE");
        try
        {
            await using var database = await sqlInstance.Build();
            await RunQuery(database, query, null, null, false, [match, other]);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public async Task Where_typed_comparisons()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              (
                where: {
                  age: {greaterThan: 20, lessThanOrEqual: 40},
                  isActive: {equal: true},
                  status: {in: [ACTIVE, PENDING]},
                  viewCount: {notEqual: 0}
                },
                orderBy: {name: ascending}
              )
              {
                name
                age
                isActive
                status
                viewCount
              }
            }
            """;

        var match = new FieldBuilderProjectionEntity
        {
            Name = "Match",
            Age = 30,
            IsActive = true,
            Status = EntityStatus.Active,
            ViewCount = 5
        };
        var tooOld = new FieldBuilderProjectionEntity
        {
            Name = "TooOld",
            Age = 41,
            IsActive = true,
            Status = EntityStatus.Active,
            ViewCount = 5
        };
        var inactive = new FieldBuilderProjectionEntity
        {
            Name = "Inactive",
            Age = 30,
            IsActive = false,
            Status = EntityStatus.Active,
            ViewCount = 5
        };
        var wrongStatus = new FieldBuilderProjectionEntity
        {
            Name = "WrongStatus",
            Age = 30,
            IsActive = true,
            Status = EntityStatus.Inactive,
            ViewCount = 5
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [match, tooOld, inactive, wrongStatus]);
    }

    [Fact]
    public async Task Where_in_with_null()
    {
        var query =
            """
            {
              withNullableEntities (where: {nullable: {in: [1, null]}}, orderBy: {nullable: ascending})
              {
                nullable
              }
            }
            """;

        var one = new WithNullableEntity
        {
            Nullable = 1
        };
        var two = new WithNullableEntity
        {
            Nullable = 2
        };
        var none = new WithNullableEntity();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [one, two, none]);
    }

    [Fact]
    public async Task Where_with_typed_variable()
    {
        var query =
            """
            query ($where: ParentEntityWhere)
            {
              parentEntities (where: $where)
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(new Dictionary<string, object?>
        {
            {
                "where", new Dictionary<string, object?>
                {
                    {
                        "property", new Dictionary<string, object?>
                        {
                            {
                                "equal", "Value2"
                            }
                        }
                    }
                }
            }
        });
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_empty_applies_no_filter()
    {
        var query =
            """
            {
              parentEntities (where: {}, orderBy: {property: ascending})
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_unknown_property()
    {
        var query =
            """
            {
              parentEntities (where: {unknown: {equal: "Value1"}})
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task Where_value_of_wrong_type()
    {
        var query =
            """
            {
              withNullableEntities (where: {nullable: {equal: "ten"}})
              {
                nullable
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task Where_comparison_not_available_for_type()
    {
        var query =
            """
            {
              withNullableEntities (where: {nullable: {startsWith: "1"}})
              {
                nullable
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task Where_owned()
    {
        var query =
            """
            {
              ownedParent (where: {child1: {property: {equal: "Child1Match"}}})
              {
                property
                child1
                {
                  property
                }
              }
            }
            """;

        var match = new OwnedParent
        {
            Property = "Match",
            Child1 = new()
            {
                Property = "Child1Match"
            },
            Child2 = new()
            {
                Property = "Child2"
            }
        };
        var other = new OwnedParent
        {
            Property = "Other",
            Child1 = new()
            {
                Property = "Child1Other"
            },
            Child2 = new()
            {
                Property = "Child2"
            }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [match, other]);
    }

    [Fact]
    public async Task Where_collection_on_navigation_list()
    {
        var query =
            """
            {
              manyToManyLeftEntities (orderBy: {leftName: ascending})
              {
                leftName
                rights (where: {lefts: {any: {leftName: {equal: "Left1"}}}})
                {
                  rightName
                }
              }
            }
            """;

        var right1 = new ManyToManyRightEntity
        {
            Id = "Right1",
            RightName = "Right1"
        };
        var right2 = new ManyToManyRightEntity
        {
            Id = "Right2",
            RightName = "Right2"
        };
        var left1 = new ManyToManyLeftEntity
        {
            Id = "Left1",
            LeftName = "Left1",
            Rights = [right1]
        };
        var left2 = new ManyToManyLeftEntity
        {
            Id = "Left2",
            LeftName = "Left2",
            Rights = [right1, right2]
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [left1, left2, right1, right2]);
    }
}
