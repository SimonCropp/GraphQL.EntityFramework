public partial class IntegrationTests
{
    [Fact]
    public async Task Collection_navigation_to_entity_with_readonly_property_falls_back_to_full_include()
    {
        var parent = new ReadOnlyParentEntity
        {
            Property = "Parent1"
        };
        var child = new ReadOnlyEntity
        {
            FirstName = "John",
            LastName = "Smith",
            Age = 25,
            ReadOnlyParent = parent,
            ReadOnlyParentId = parent.Id
        };
        parent.Children.Add(child);

        var query =
            """
            {
              readOnlyParentEntities
              {
                property
                children
                {
                  firstName
                  computedInDb
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    [Fact]
    public async Task Single_query_with_collection_navigation_to_readonly_entity()
    {
        var parent = new ReadOnlyParentEntity
        {
            Property = "TheParent"
        };
        var child = new ReadOnlyEntity
        {
            FirstName = "Jane",
            LastName = "Doe",
            Age = 30,
            ReadOnlyParent = parent,
            ReadOnlyParentId = parent.Id
        };
        parent.Children.Add(child);

        var query =
            $$"""
            {
              readOnlyParentEntity(id: "{{parent.Id}}")
              {
                property
                children
                {
                  firstName
                  lastName
                  computedInDb
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    [Fact]
    public void Projection_with_readonly_scalar_on_collection_navigation_builds_expression()
    {
        // When a collection navigation target has a read-only property,
        // TryBuild should succeed by falling back to including the full navigation entity.
        var navigationProjection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "ComputedInDb" },
            ["Id"],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ReadOnlyParentId" },
            null);

        var projection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "Property" },
            ["Id"],
            null,
            new()
            {
                ["Children"] = new(typeof(ReadOnlyEntity), true, navigationProjection)
            });

        var keyNames = new Dictionary<Type, List<string>>
        {
            [typeof(ReadOnlyParentEntity)] = ["Id"],
            [typeof(ReadOnlyEntity)] = ["Id"]
        };

        var result = SelectExpressionBuilder.TryBuild<ReadOnlyParentEntity>(projection, keyNames, out var expression);

        Assert.True(result);
        Assert.NotNull(expression);
    }

    [Fact]
    public void Projection_with_readonly_scalar_on_single_navigation_builds_expression()
    {
        // When a single navigation target has a read-only property,
        // TryBuild should succeed by falling back to including the full navigation entity.
        var navigationProjection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "Property" },
            ["Id"],
            null,
            null);

        var projection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "FirstName" },
            ["Id"],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ReadOnlyParentId" },
            new()
            {
                ["ReadOnlyParent"] = new(typeof(ReadOnlyParentEntity), false, navigationProjection)
            });

        var keyNames = new Dictionary<Type, List<string>>
        {
            [typeof(ReadOnlyEntity)] = ["Id"],
            [typeof(ReadOnlyParentEntity)] = ["Id"]
        };

        var result = SelectExpressionBuilder.TryBuild<ReadOnlyEntity>(projection, keyNames, out var expression);

        Assert.True(result);
        Assert.NotNull(expression);
    }

    [Fact]
    public void Projection_with_readonly_scalar_at_root_still_returns_null()
    {
        // A read-only property at the ROOT entity level
        // still causes TryBuild to return null (load full entity).
        var projection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "FirstName", "ComputedInDb" },
            ["Id"],
            null,
            null);

        var keyNames = new Dictionary<Type, List<string>>
        {
            [typeof(ReadOnlyEntity)] = ["Id"]
        };

        var result = SelectExpressionBuilder.TryBuild<ReadOnlyEntity>(projection, keyNames, out var expression);

        Assert.False(result);
        Assert.Null(expression);
    }
}
