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


## Adding Filters

All filters are added using the `For<TEntity>()` fluent API, which automatically infers the projection type. This provides a consistent interface regardless of whether filtering on a single field, multiple fields with anonymous types, or using named projection classes.


### Basic Syntax:

```csharp
var filters = new Filters<MyDbContext>();

filters.For<EntityType>().Add(
    projection: entity => /* projection expression */,
    filter: (userContext, dbContext, userPrincipal, projected) => /* filter logic */);
```

### How It Works:

1. Call `For<TEntity>()` to specify the entity type
2. Call `Add()` with a projection expression and filter function
3. The compiler automatically infers the projection type from the expression
4. Filter projection expression is analyzed to extract accessed property names
5. GraphQL query executes and loads entities (including properties needed by the filter)
6. For each loaded entity, the projection expression is compiled and executed in-memory
7. The projected data is passed to the filter function
8. Entities that fail the filter are excluded from results

**Note**: The projection is executed in-memory on entities that have already been loaded from the database by the GraphQL query. It is not a separate database query.


## Single Field Filters

For filtering based on a single property value, project directly to that property:

snippet: value-type-projections


## Multi-Field Filters with Anonymous Types

For filtering based on multiple fields, use anonymous types without needing to define projection classes:

snippet: filter-all-fields

Anonymous types provide a concise way to combine multiple fields for filtering logic.


## Named Projection Types

For reusable filter logic or complex projections, define a named projection class:

snippet: projection-filter

Named types are useful when:

* The same projection is used in multiple filters
* The projection includes nested objects or computed properties
* A descriptive type name aids code documentation


## Nullable Types

Filters fully support nullable types for both value types and reference types:

snippet: nullable-value-type-projections

Common nullable patterns:

* **Has value check**: `quantity.HasValue && quantity.Value > 0`
* **Null check**: `!quantity.HasValue`
* **Exact match**: `isApproved == true` (not null or false)


## Async Filters

Filters can be asynchronous when they need to perform database lookups or other async operations:

snippet: async-filter


## Navigation Properties

Filters can project through navigation properties to access related entity data:

snippet: navigation-property-filter


## Boolean Expression Shorthand

For boolean properties, a simplified syntax is available where only the filter expression is needed:

snippet: boolean-expression-filter

This shorthand is useful when:

* Filtering on a single boolean property
* The filter condition checks if the property is true
* A concise syntax is preferred

The expression `filter: _ => _.IsActive` is automatically expanded to use the boolean property as both the projection and the filter condition.


## Filters Without Projection

For filters that don't need entity data (such as authorization checks based only on user context), a projection-less syntax is available:

snippet: filter-without-projection

This overload is useful when:

* Checking user permissions or claims
* Applying access control based on user context
* No entity data is needed for the filter decision

The filter receives `userContext`, `dbContext`, and `userPrincipal` but does not receive any entity data.


## Async Filters Without Projection

Filters without projection can also be asynchronous for operations that require database lookups:

snippet: async-filter-without-projection

This is useful when:

* User permissions are stored in the database
* Filter logic requires async operations
* Complex checks involve multiple data sources


## Simplified Filter API

For filters that only need to access **primary key** or **foreign key** properties, a simplified API is available that eliminates the need to specify a projection:

snippet: simplified-filter-api

### When to Use the Simplified API

Use `filters.For<TEntity>().Add(filter: (_, _, _, e) => ...)` when the filter **only** accesses:

* **Primary keys**: `Id`, `EntityId`, `CompanyId` (matching the entity type name)
* **Foreign keys**: Properties ending with `Id` like `ParentId`, `CategoryId`, `LocationId`

The simplified API uses identity projection (`_ => _`) internally, which in EF projections only guarantees that key properties are loaded.

### Key Property Detection Rules

A property is considered a **primary key** if it is:

* Named `Id`
* Named `{TypeName}Id` (e.g., `CompanyId` for `Company` entity)
* Named `{TypeName}Id` where TypeName has suffix removed: `Entity`, `Model`, `Dto`
  * Example: `CompanyId` in `CompanyEntity` class

A property is considered a **foreign key** if:

* Name ends with `Id` (but is not solely `Id`)
* Not identified as a primary key
* Type is `int`, `long`, `short`, or `Guid` (nullable or non-nullable)

### Restrictions

**IMPORTANT**: Do not access scalar properties (like `Name`, `City`, `Capacity`) or navigation properties (like `Parent`, `Category`) with the simplified API. These properties are not loaded by identity projection and will cause runtime errors.

For non-key properties, use the full API with explicit projection:

```csharp
// INVALID - Will cause runtime error
filters.For<Accommodation>().Add(
    filter: (_, _, _, a) => a.City == "London");  // City is NOT a key

// VALID - Explicit projection for scalar properties
filters.For<Accommodation>().Add(
    projection: a => a.City,
    filter: (_, _, _, city) => city == "London");
```

### Comparison with Full API

The simplified API is syntactic sugar for the identity projection pattern:

```csharp
// Simplified API
filters.For<Accommodation>().Add(
    filter: (_, _, _, a) => a.Id != Guid.Empty);

// Equivalent full API
filters.For<Accommodation>().Add(
    projection: _ => _,  // Identity projection
    filter: (_, _, _, a) => a.Id != Guid.Empty);
```

### Analyzer Support

Three analyzer diagnostics help ensure correct usage:

* **GQLEF004** (Info): Suggests using the simplified API when identity projection only accesses keys
* **GQLEF005** (Error): Prevents accessing non-key properties with simplified API
* **GQLEF006** (Error): Prevents accessing non-key properties with identity projection

### Migration Guide

Existing code using identity projection with filters that only access keys can be migrated to the simplified API:

**Before:**
```csharp
filters.For<Product>().Add(
    projection: _ => _,
    filter: (_, _, _, p) => p.CategoryId == allowedCategoryId);
```

**After:**
```csharp
filters.For<Product>().Add(
    filter: (_, _, _, p) => p.CategoryId == allowedCategoryId);
```

The simplified API makes intent clearer and reduces boilerplate while maintaining the same runtime behavior
