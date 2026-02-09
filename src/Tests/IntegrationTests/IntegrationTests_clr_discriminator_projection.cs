public partial class IntegrationTests
{
    /// <summary>
    /// Verifies that when querying a concrete derived type in a TPH hierarchy with a CLR
    /// discriminator property, the discriminator column is included in the SQL SELECT projection
    /// even when it is not explicitly requested in the GraphQL query.
    /// </summary>
    [Fact]
    public async Task CLR_discriminator_included_in_single_field_projection()
    {
        var entity = new DiscriminatorDerivedAEntity
        {
            Property = "Value1",
            DerivedAProperty = "DerivedA1"
        };

        var query =
            $$"""
            {
              discriminatorDerivedAEntity(id: "{{entity.Id}}")
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    /// <summary>
    /// Verifies that the CLR discriminator property returns the correct enum value
    /// when explicitly requested in the GraphQL query, not the default enum value.
    /// Without the fix, the projected entity would have EntityType = TypeA (default 0)
    /// regardless of the actual discriminator value.
    /// </summary>
    [Fact]
    public async Task CLR_discriminator_returns_correct_value_when_queried()
    {
        var entityA = new DiscriminatorDerivedAEntity
        {
            Property = "ValueA",
            DerivedAProperty = "DerivedA"
        };
        var entityB = new DiscriminatorDerivedBEntity
        {
            Property = "ValueB",
            DerivedBProperty = "DerivedB"
        };

        var query =
            $$"""
            {
              discriminatorDerivedBEntity(id: "{{entityB.Id}}")
              {
                entityType
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entityA, entityB]);
    }

    /// <summary>
    /// Verifies that the CLR discriminator column is included in projection when querying
    /// a list of derived entities, ensuring all returned entities have the correct
    /// discriminator value.
    /// </summary>
    [Fact]
    public async Task CLR_discriminator_included_in_query_field_projection()
    {
        var entity1 = new DiscriminatorDerivedAEntity
        {
            Property = "First",
            DerivedAProperty = "DerivedA1"
        };
        var entity2 = new DiscriminatorDerivedAEntity
        {
            Property = "Second",
            DerivedAProperty = "DerivedA2"
        };
        // This TypeB entity should not appear in TypeA query results
        var entity3 = new DiscriminatorDerivedBEntity
        {
            Property = "Third",
            DerivedBProperty = "DerivedB1"
        };

        var query =
            """
            {
              discriminatorDerivedAEntities
              {
                entityType
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }
}
