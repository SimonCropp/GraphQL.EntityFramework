public partial class IntegrationTests
{
    [Fact]
    public async Task Query_entity_with_guarded_key_setter()
    {
        var entity = new GuardedKeyEntity("test@example.com");

        var query =
            """
            {
              guardedKeyEntities
              {
                id
                emailAddress
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Single_entity_with_guarded_key_setter()
    {
        var entity = new GuardedKeyEntity("test@example.com");

        var query =
            $$"""
            {
              guardedKeyEntity(id: "{{entity.Id}}")
              {
                id
                emailAddress
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public void Projection_with_guarded_key_setter_returns_null()
    {
        // When a key property has a custom (non-auto) setter,
        // TryBuild should return null to fall back to full entity loading,
        // because MemberInit expressions call property setters which would throw.
        var projection = new FieldProjectionInfo(
            new(StringComparer.OrdinalIgnoreCase) { "EmailAddress" },
            ["Id"],
            null,
            null);

        var keyNames = new Dictionary<Type, List<string>>
        {
            [typeof(GuardedKeyEntity)] = ["Id"]
        };

        var result = SelectExpressionBuilder.TryBuild<GuardedKeyEntity>(projection, keyNames, out var expression);

        Assert.False(result);
        Assert.Null(expression);
    }
}
