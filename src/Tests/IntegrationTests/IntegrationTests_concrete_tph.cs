public partial class IntegrationTests
{
    // A member init creates the type it names, so projecting a concrete base type materialized
    // every row as the base and derived rows lost their identity: the derived fragment matched
    // nothing and __typename was wrong. Types with derived types now load the full entity instead.
    [Fact]
    public async Task Concrete_tph_base_keeps_derived_identity()
    {
        var query =
            """
            {
              concreteTphEntities
              {
                __typename
                property
                ... on ConcreteTphDerived
                {
                  derivedProperty
                }
              }
            }
            """;

        var baseEntity = new ConcreteTphBaseEntity
        {
            Property = "Value1"
        };
        var derivedEntity = new ConcreteTphDerivedEntity
        {
            Property = "Value2",
            DerivedProperty = "Derived"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [baseEntity, derivedEntity]);
    }

    [Fact]
    public async Task Concrete_tph_base_keeps_derived_identity_single()
    {
        var derivedEntity = new ConcreteTphDerivedEntity
        {
            Property = "Value1",
            DerivedProperty = "Derived"
        };

        var query =
            $$"""
            {
              concreteTphEntity(id: "{{derivedEntity.Id}}")
              {
                __typename
                property
                ... on ConcreteTphDerived
                {
                  derivedProperty
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [derivedEntity]);
    }
}
