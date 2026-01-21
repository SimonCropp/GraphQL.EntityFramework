public partial class IntegrationTests
{
    [Fact]
    public async Task FieldBuilder_int_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                age
                ageDoubled
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "John",
            Age = 25
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_bool_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                age
                isAdult
              }
            }
            """;

        var entity1 = new FieldBuilderProjectionEntity
        {
            Name = "Child",
            Age = 15
        };
        var entity2 = new FieldBuilderProjectionEntity
        {
            Name = "Adult",
            Age = 25
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task FieldBuilder_datetime_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                createdAt
                createdYear
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Test",
            CreatedAt = new(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_decimal_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                salary
                salaryWithBonus
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Employee",
            Salary = 50000.00m
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_double_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                score
                scoreNormalized
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Student",
            Score = 85.5
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_long_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                viewCount
                viewCountDoubled
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Video",
            ViewCount = 1234567890L
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_async_bool_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                hasParentAsync
              }
            }
            """;

        var parent = new FieldBuilderProjectionParentEntity
        {
            Name = "Parent"
        };
        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Child",
            Parent = parent
        };
        parent.Children.Add(entity);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity, parent]);
    }

    [Fact]
    public async Task FieldBuilder_async_int_value_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                siblingCountAsync
              }
            }
            """;

        var parent = new FieldBuilderProjectionParentEntity
        {
            Name = "Parent"
        };
        var entity1 = new FieldBuilderProjectionEntity
        {
            Name = "Child1",
            Parent = parent
        };
        var entity2 = new FieldBuilderProjectionEntity
        {
            Name = "Child2",
            Parent = parent
        };
        var entity3 = new FieldBuilderProjectionEntity
        {
            Name = "Child3",
            Parent = parent
        };
        parent.Children.Add(entity1);
        parent.Children.Add(entity2);
        parent.Children.Add(entity3);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, parent]);
    }

    [Fact]
    public async Task FieldBuilder_string_reference_type_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                nameUpper
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "lowercase"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_navigation_property_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                parentName
              }
            }
            """;

        var parent = new FieldBuilderProjectionParentEntity
        {
            Name = "ParentName"
        };
        var entityWithParent = new FieldBuilderProjectionEntity
        {
            Name = "HasParent",
            Parent = parent
        };
        var entityWithoutParent = new FieldBuilderProjectionEntity
        {
            Name = "NoParent",
            Parent = null
        };
        parent.Children.Add(entityWithParent);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entityWithParent, entityWithoutParent, parent]);
    }

    [Fact]
    public async Task FieldBuilder_multiple_value_types_combined()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                age
                ageDoubled
                isAdult
                salary
                salaryWithBonus
                score
                scoreNormalized
                viewCount
                viewCountDoubled
                nameUpper
              }
            }
            """;

        var entity = new FieldBuilderProjectionEntity
        {
            Name = "Complete",
            Age = 30,
            Salary = 75000.00m,
            Score = 92.5,
            ViewCount = 999888777L
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task FieldBuilder_enum_scalar_projection()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities
              {
                name
                status
                statusDisplay
              }
            }
            """;

        var entity1 = new FieldBuilderProjectionEntity
        {
            Name = "ActiveEntity",
            Status = EntityStatus.Active
        };
        var entity2 = new FieldBuilderProjectionEntity
        {
            Name = "PendingEntity",
            Status = EntityStatus.Pending
        };
        var entity3 = new FieldBuilderProjectionEntity
        {
            Name = "InactiveEntity",
            Status = EntityStatus.Inactive
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task FieldBuilder_enum_projection_through_navigation()
    {
        // This test verifies that scalar enum projections work correctly when navigating through relationships
        // Before the fix, this would fail with: "Unable to find navigation 'Status' specified in string based include path"
        // because IncludeAppender tried to add includes for scalar types
        var query =
            """
            {
              fieldBuilderProjectionParents
              {
                name
                children {
                  edges {
                    node {
                      name
                      status
                      statusDisplay
                    }
                  }
                }
              }
            }
            """;

        var parent = new FieldBuilderProjectionParentEntity
        {
            Name = "Parent"
        };
        var child1 = new FieldBuilderProjectionEntity
        {
            Name = "ActiveChild",
            Status = EntityStatus.Active,
            Parent = parent
        };
        var child2 = new FieldBuilderProjectionEntity
        {
            Name = "PendingChild",
            Status = EntityStatus.Pending,
            Parent = parent
        };
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }
}
