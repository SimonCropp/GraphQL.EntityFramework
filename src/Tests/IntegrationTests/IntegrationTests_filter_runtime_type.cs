public partial class IntegrationTests
{
    // Field<TGraphType> returns an object typed builder. The filter lookup used the static type,
    // so no filter matched object and a denied parent was returned through parentObject while the
    // typed parentAlias next to it was correctly null.
    [Fact]
    public async Task Filter_applies_to_object_typed_projection_field()
    {
        var query =
            """
            {
              childEntities
              {
                property
                parentAlias
                {
                  property
                }
                parentObject
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Ignore"
        };
        var child = new ChildEntity
        {
            Property = "Value1",
            Parent = parent
        };
        parent.Children.Add(child);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ParentEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Ignore");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [parent, child]);
    }

    // A filter registered for a derived type was only applied to fields typed as that derived
    // type. Derived instances returned by a base typed field went unfiltered.
    [Fact]
    public async Task Filter_on_derived_type_applies_to_base_typed_list()
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
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Ignore");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [derived, derivedKept, derivedWithNavigation]);
    }

    // The single field variant, so the runtime type is used for a lone item as well as a list
    [Fact]
    public async Task Filter_on_derived_type_applies_to_base_typed_single()
    {
        var leaf = new TphLeafEntity
        {
            Property = "Ignore",
            LeafProperty = "Leaf"
        };

        var query =
            $$"""
            {
              tphMiddleEntity(id: "{{leaf.Id}}")
              {
                property
              }
            }
            """;

        var filters = new Filters<IntegrationDbContext>();
        filters.For<TphLeafEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Ignore");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [leaf]);
    }

    // The projection requirements of a derived type filter are loaded by a base typed query, so
    // the filter can read them from the derived items it is applied to.
    [Fact]
    public async Task Filter_on_derived_type_requirements_loaded_by_base_typed_query()
    {
        var query =
            """
            {
              interfaceGraphConnection(first: 10)
              {
                items
                {
                  property
                }
              }
            }
            """;

        var derived = new DerivedEntity
        {
            Property = "Value1",
            Status = "Hidden"
        };
        var derivedKept = new DerivedEntity
        {
            Property = "Value2",
            Status = "Visible"
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Status,
            filter: (_, _, _, status) => status != "Hidden");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, filters, [derived, derivedKept]);
    }

    static Task RunQuery(SqlDatabase<IntegrationDbContext> database, string query, Filters<IntegrationDbContext> filters, object[] entities, [CallerFilePath] string sourceFile = "") =>
        RunQuery(database, query, null, filters, false, entities, sourceFile: sourceFile);
}
