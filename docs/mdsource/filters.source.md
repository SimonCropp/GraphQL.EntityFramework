# Filters

Sometimes, in the context of constructing an EF query, it is not possible to know if any given item should be returned in the results. For example when performing authorization where the rules are pulled from a different system, and that information does not exist in the database.

`Filters` allows a custom function to be executed after the EF query execution and determine if any given node should be included in the result.

Notes:

 * When evaluated on nodes of a collection, excluded nodes will be removed from collection.
 * When evaluated on a property node, the value will be replaced with null.
 * When doing paging or counts, there is currently no smarts that adjust counts or pages sizes when items are excluded. If this is required submit a PR that adds this feature, or don't mix filters with paging.
 * The filter is passed the current [User Context](https://graphql-dotnet.github.io/docs/getting-started/user-context) and the node item instance.
 * Filters will not be executed on null item instance.
 * A [Type.IsAssignableFrom](https://docs.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom) check will be performed to determine if an item instance should be filtered based on the `<TItem>`.


### Signature:

snippet: FiltersSignature


## Filter Projections

Filter projections transform entity data before passing it to the filter function. This is useful when filtering based on specific properties or computed values.

### Why Use Projections?

Projections serve two purposes:

1. **Transform Data**: Project entities to custom types containing only the fields needed for filtering
2. **Declare Dependencies**: The library analyzes the projection expression to identify which entity properties are accessed

**Benefits:**

* **Cleaner Filter Logic**: Filter functions receive only the data they need, not entire entities
* **Type Safety**: Projection types clearly define what data filters depend on
* **Flexible Filtering**: Can project to custom objects, value types, or computed values
* **Explicit Dependencies**: Clearly declare what data the filter requires

### Usage:

snippet: projection-filter

### How It Works:

1. Filter projection expression is analyzed to extract accessed property names
2. GraphQL query executes and loads entities (including properties needed by the filter)
3. For each loaded entity, the projection expression is compiled and executed in-memory
4. The projected data is passed to the filter function
5. Entities that fail the filter are excluded from results

**Note**: The projection is executed in-memory on entities that have already been loaded from the database by the GraphQL query. It is not a separate database query.

### Filtering on Multiple Fields:

To filter using multiple entity fields, explicitly list all fields in the projection:

snippet: filter-all-fields


## Value Type Projections

For filtering scenarios where only a single property value needs to be checked, projecting directly to a value type is an option instead of creating a dedicated projection class. This is useful for basic authorization rules or business logic that depends on a single field.

### Supported Value Types:

The filter projection system supports all value types including:

* Primitive types: `string`, `int`, `bool`, `decimal`, `double`, etc.
* Date/time types: `DateTime`, `DateTimeOffset`, `TimeSpan`
* Other value types: `Guid`, enums, custom structs

### Usage:

snippet: value-type-projections

### Benefits:

* **Less code**: No need to create projection classes for single-field filters
* **Type safety**: The filter function receives the strongly-typed value
* **Same mechanism**: Uses the same in-memory projection compilation as object projections
* **Combines with object projections**: Value type and object projections can be mixed in the same filter collection

### When to Use:

* **Single field checks**: Filtering based on one property (status, age, type)
* **Simple comparisons**: Equality, range checks, null checks
* **Quick authorization**: Checking if a single field matches allowed values

### When to Use Object Projections Instead:

* **Multiple fields**: Need to check multiple properties in combination
* **Complex logic**: Filtering requires multiple related values
* **Navigation properties**: Need to access foreign keys or related data


## Nullable Value Type Projections

Value type projections fully support nullable types, allowing filters to handle both null checks and value-based filtering.

### Supported Nullable Types:

* Nullable value types: `int?`, `bool?`, `decimal?`, `double?`, etc.
* Nullable date/time types: `DateTime?`, `DateTimeOffset?`, `TimeSpan?`
* Nullable reference types: `string?` (reference types are nullable by default)
* Other nullable types: `Guid?`, nullable enums, nullable custom structs

### Usage:

snippet: nullable-value-type-projections

### Common Patterns:

* **Has value check**: `quantity.HasValue && quantity.Value > 0` - Filter items where nullable has a value meeting criteria
* **Null check**: `!quantity.HasValue` - Filter items where value is null
* **Exact match**: `isApproved == true` - Filter items where nullable bool is exactly true (not null or false)
* **Null coalescing**: Can use null-conditional operators in filter logic

### Benefits:

* **Explicit null handling**: Clearly express intent for null vs non-null filtering
* **Type safety**: Compiler ensures correct nullable handling
* **Flexible filtering**: Can filter on presence/absence of values or the values themselves


## Convenience Overloads for Common Types

For commonly-used primitive and value types, convenience overloads are available that automatically infer the projection type, reducing verbosity when adding filters.

### Supported Types:

**Value types** (both nullable and non-nullable):
* Numeric: `bool`, `byte`, `sbyte`, `char`, `decimal`, `double`, `float`, `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`, `ushort`
* Date/Time: `DateTime`, `DateTimeOffset`, `TimeOnly`, `DateOnly`, `TimeSpan`
* Identifier: `Guid`

**Reference types** (nullable):
* `string?`

### Usage:

**Before** (explicit type parameter):
```csharp
filters.Add<ChildEntity, int>(
    projection: _ => _.Age,
    filter: (_, _, _, age) => age >= 18);
```

**After** (inferred type parameter):
```csharp
filters.Add<ChildEntity>(
    projection: _ => _.Age,
    filter: (_, _, _, age) => age >= 18);
```

### Examples:

**Integer filter:**
```csharp
filters.Add<Product>(
    projection: _ => _.Quantity,
    filter: (_, _, _, qty) => qty > 0);
```

**Nullable integer filter:**
```csharp
filters.Add<Order>(
    projection: _ => _.DiscountPercent,
    filter: (_, _, _, discount) => discount.HasValue && discount.Value >= 10);
```

**String filter:**
```csharp
filters.Add<User>(
    projection: _ => _.Status,
    filter: (_, _, _, status) => status != "Suspended");
```

**DateTime filter:**
```csharp
filters.Add<Article>(
    projection: _ => _.PublishedDate,
    filter: (_, _, _, date) => date >= DateTime.UtcNow.AddDays(-30));
```

**Boolean filter:**
```csharp
filters.Add<Account>(
    projection: _ => _.IsActive,
    filter: (_, _, _, isActive) => isActive);
```

**Guid filter:**
```csharp
filters.Add<Document>(
    projection: _ => _.OwnerId,
    filter: (userContext, _, _, ownerId) => ownerId == (Guid)userContext);
```

**Async filter:**
```csharp
filters.Add<Product>(
    projection: _ => _.CategoryId,
    filter: async (_, dbContext, _, categoryId) =>
    {
        var category = await dbContext.Categories.FindAsync(categoryId);
        return category?.IsVisible == true;
    });
```

### Benefits:

* **Less typing**: No need to specify `TProjection` type parameter
* **Cleaner code**: Reduced visual clutter in filter registration
* **Type safety**: Full type inference maintains compile-time safety
* **Same performance**: Zero overhead - delegates to existing generic methods
* **Works with async**: Both `Filter<T>` and `AsyncFilter<T>` delegates supported