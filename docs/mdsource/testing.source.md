# Testing Filters

Entity filters can be tested directly without running a full GraphQL query. The `Filters<TDbContext>.ShouldInclude` method evaluates registered filters against a single entity instance, returning a boolean result.

This approach is significantly faster than executing GraphQL queries in tests because it bypasses schema compilation, query parsing, and the full GraphQL execution pipeline.


## Signature

snippet: ShouldIncludeSignature


## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `userContext` | `object` | The GraphQL user context. Use `new Dictionary<string, object?>()` in tests |
| `data` | `TDbContext` | The `DbContext` instance |
| `user` | `ClaimsPrincipal?` | The user to evaluate filters against |
| `item` | `object` | The entity instance. Navigation properties used by filter projections must be loaded |


## How It Works

1. Uses `item.GetType()` to determine the entity type at runtime
2. Finds all registered filters that apply to that type (including base type filters via `IsAssignableFrom`)
3. For each matching filter, applies the compiled projection expression to the entity
4. Passes the projected data to the filter function
5. Returns `false` as soon as any filter excludes the entity, or `true` if all filters pass


## Usage

snippet: testing-should-include


## Loading Navigation Properties

Filter projections often traverse navigation properties (e.g. `_.Category.Name` or `_.TravelRequest.GroupOwnerId`). When loading an entity with `FindAsync`, these navigations are not populated by default. They must be explicitly loaded before calling `ShouldInclude`, otherwise the projection will see `null` values.

The recommended pattern loads all reference (non-collection) navigations:

```csharp
var entry = dbContext.Entry(entity);
foreach (var nav in entry.Navigations)
{
    if (nav.Metadata is INavigation { IsCollection: false } &&
        !nav.IsLoaded)
    {
        await nav.LoadAsync();
    }
}
```

Notes:

 * Only **reference** navigations (`IsCollection: false`) need loading. Collection navigations are not traversed by filter projections.
 * Some filters perform their own database queries internally (e.g. using `data.Set<T>().Where(...).Select(...)`) and do not rely on navigation properties being pre-loaded.
 * If a filter uses an identity projection (`_ => _`), only key properties are accessed, so no navigation loading is needed.


## Testing Multiple Entities

To test filter behavior across many entity types and user roles, combine `ShouldInclude` with parameterized tests:

```csharp
[Test]
[TestCaseSource(nameof(GetTestCases))]
public async Task Permissions(Type entityType, Guid entityId, ClaimsPrincipal user)
{
    var filters = new Filters<MyDbContext>();
    MyFilters.AddFilters(filters);

    var entity = await dbContext.FindAsync(entityType, entityId);

    // load navigations...

    var result = await filters.ShouldInclude(
        new Dictionary<string, object?>(),
        dbContext,
        user,
        entity!);

    await Verify(result);
}
```

This avoids the overhead of compiling a GraphQL schema and building a service provider for each test case.
