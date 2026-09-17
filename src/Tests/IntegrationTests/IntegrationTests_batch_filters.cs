// Batch filters: the items of a response are filtered with one call per batch filter per query
// depth, whichever row or field returned them. Each call is recorded, so the snapshots show how
// many calls the items shared.
public partial class IntegrationTests
{
    [Fact]
    public async Task Batch_filter_root_list()
    {
        var query =
            """
            {
              parentEntities
              {
                property
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Ignore"
        };
        var parent3 = new ParentEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), [parent1, parent2, parent3]);
    }

    // Two root fields returning the same type share the call
    [Fact]
    public async Task Batch_filter_fields_share_one_call()
    {
        var query =
            """
            {
              first: parentEntities(where: {property: {equal: "Value1"}})
              {
                property
              }
              second: parentEntities(where: {property: {notEqual: "Value1"}})
              {
                property
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Ignore"
        };
        var parent3 = new ParentEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), [parent1, parent2, parent3]);
    }

    // The children of each parent resolve separately, and are filtered in one call
    [Fact]
    public async Task Batch_filter_navigation_list_rows_share_one_call()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children
                {
                  property
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    // The parent of each child resolves separately, and is filtered in one call
    [Fact]
    public async Task Batch_filter_navigation_rows_share_one_call()
    {
        var query =
            """
            {
              childEntities
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var kept = new ParentEntity
        {
            Property = "Value1"
        };
        var ignored = new ParentEntity
        {
            Property = "Ignore"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = kept
        };
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = ignored
        };
        var child3 = new ChildEntity
        {
            Property = "Child3",
            Parent = kept
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), [kept, ignored, child1, child2, child3]);
    }

    [Fact]
    public async Task Batch_filter_navigation_connection_rows_share_one_call()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                childrenConnection
                {
                  totalCount
                  items
                  {
                    property
                  }
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    // The total count is taken before any filter runs, as it is for a per item filter
    [Fact]
    public async Task Batch_filter_root_connection()
    {
        var query =
            """
            {
              parentEntitiesConnection(first: 10)
              {
                totalCount
                items
                {
                  property
                  children
                  {
                    property
                  }
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_resolve_list_rows_share_one_call()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                childrenViaResolveList
                {
                  property
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_resolve_rows_share_one_call()
    {
        var query =
            """
            {
              childEntities
              {
                property
                parentObject
                {
                  property
                }
              }
            }
            """;

        var kept = new ParentEntity
        {
            Property = "Value1"
        };
        var ignored = new ParentEntity
        {
            Property = "Ignore"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = kept
        };
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = ignored
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), [kept, ignored, child1, child2]);
    }

    // Each depth of the query shares one call per batch filter, however many rows it has
    [Fact]
    public async Task Batch_filter_one_call_per_depth()
    {
        var query =
            """
            {
              childEntities
              {
                property
                parent
                {
                  property
                  children
                  {
                    property
                    parent
                    {
                      property
                    }
                  }
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_single()
    {
        var (entities, kept, _) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntity(id: "{{kept.Id}}")
                {
                  property
                  children
                  {
                    property
                  }
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_single_excluded()
    {
        var (entities, _, ignored) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntity(id: "{{ignored.Id}}")
                {
                  property
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_single_nullable_excluded()
    {
        var (entities, _, ignored) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntityNullable(id: "{{ignored.Id}}")
                {
                  property
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_first()
    {
        var (entities, kept, _) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntityFirst(id: "{{kept.Id}}")
                {
                  property
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_first_excluded()
    {
        var (entities, _, ignored) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntityFirst(id: "{{ignored.Id}}")
                {
                  property
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    [Fact]
    public async Task Batch_filter_first_nullable_excluded()
    {
        var (entities, _, ignored) = BuildParentsWithChildren();
        var query =
            $$"""
              {
                parentEntityNullableFirst(id: "{{ignored.Id}}")
                {
                  property
                }
              }
              """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, BuildBatchFilters(), entities);
    }

    // Items a per item filter excludes are not passed to the batch filter
    [Fact]
    public async Task Batch_filter_runs_after_per_item_filters()
    {
        var query =
            """
            {
              parentEntities
              {
                property
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Ignore"
        };
        var parent3 = new ParentEntity
        {
            Property = "PerItemIgnore"
        };

        var filters = BuildBatchFilters();
        filters.For<ParentEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "PerItemIgnore");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [parent1, parent2, parent3]);
    }

    [Fact]
    public async Task Batch_filter_on_derived_type_applies_to_base_typed_list()
    {
        var query =
            """
            {
              baseEntities
              {
                property
              }
            }
            """;

        var derived = new DerivedEntity
        {
            Property = "Ignore"
        };
        var derivedKept = new DerivedEntity
        {
            Property = "Value1"
        };
        var derivedWithNavigation = new DerivedWithNavigationEntity
        {
            Property = "Value2"
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().AddBatch(
            projection: _ => _.Property,
            filter: (_, _, _, properties) => ExcludeIgnored("derivedBatch", properties));

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [derived, derivedKept, derivedWithNavigation]);
    }

    [Fact]
    public async Task Batch_filter_exception()
    {
        var query =
            """
            {
              parentEntities
              {
                property
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ParentEntity>().AddBatch<string?>(
            projection: _ => _.Property,
            filter: (_, _, _, _) => throw new("Batch filter failed"));

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [parent]);
    }

    static Filters<IntegrationDbContext> BuildBatchFilters()
    {
        var filters = new Filters<IntegrationDbContext>();
        filters.For<ParentEntity>().AddBatch(
            projection: _ => _.Property,
            filter: (_, _, _, properties) => ExcludeIgnored("parentBatch", properties));
        filters.For<ChildEntity>().AddBatch(
            projection: _ => _.Property,
            filter: (_, _, _, properties) => ExcludeIgnored("childBatch", properties));
        return filters;
    }

    // Records the call, so the snapshot shows how many calls there were and what each was passed
    static Task<IReadOnlySet<string?>> ExcludeIgnored(string name, IReadOnlyCollection<string?> properties)
    {
        Recording.Add(name, properties.Order().ToList());
        IReadOnlySet<string?> included = properties
            .Where(_ => _ != "Ignore")
            .ToHashSet();
        return Task.FromResult(included);
    }

    // Two parents, each with a kept and an ignored child, and an ignored parent with a kept child
    static (object[] Entities, ParentEntity Kept, ParentEntity Ignored) BuildParentsWithChildren()
    {
        var parent1 = new ParentEntity
        {
            Property = "Parent1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Parent2"
        };
        var ignored = new ParentEntity
        {
            Property = "Ignore"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        var child1Ignored = new ChildEntity
        {
            Property = "Ignore",
            Parent = parent1
        };
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent2
        };
        var child2Ignored = new ChildEntity
        {
            Property = "Ignore",
            Parent = parent2
        };
        var child3 = new ChildEntity
        {
            Property = "Child3",
            Parent = ignored
        };
        parent1.Children.Add(child1);
        parent1.Children.Add(child1Ignored);
        parent2.Children.Add(child2);
        parent2.Children.Add(child2Ignored);
        ignored.Children.Add(child3);

        return (
            [parent1, parent2, ignored, child1, child1Ignored, child2, child2Ignored, child3],
            parent1,
            ignored);
    }
}
