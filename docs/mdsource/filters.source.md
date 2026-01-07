# Filters

Sometimes, in the context of constructing an EF query, it is not possible to know if any given item should be returned in the results. For example when performing authorization where the rules rules are pulled from a different system, and that information does not exist in the database.

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

When filters need to access navigation properties or foreign keys that aren't included in the GraphQL query, the library provides projection-based filter overloads. These overloads allow specifying exactly which fields the filter needs, and the library will efficiently query only those fields from the database.

### Why Use Projections?

Without projections, filters receive the entity instance as loaded by the GraphQL query. If the query only selects a few fields (via EF projection), foreign keys and navigation properties may not be populated, making authorization decisions impossible.

**Benefits:**

* **Access to Foreign Keys**: Query foreign key properties even when not requested in the GraphQL query
* **Performance**: Load only the fields needed for filtering, not the entire entity
* **Explicit Dependencies**: Clearly declare what data the filter requires

**Important Requirements:**

* The entity type must have an `Id` property
* The projection is executed as a separate database query using the entity IDs
* The `Id` is automatically included in the database query even if not in the projection

### Usage:

snippet: projection-filter

### How It Works:

1. GraphQL query executes and loads entities based on requested fields
2. For each entity, the library extracts the entity ID
3. A separate database query fetches the projected fields (including Id automatically) using those IDs:
   ```sql
   SELECT Id, ParentId
   FROM ChildEntities
   WHERE Id IN (...)
   ```
4. The projected data is passed to the filter function
5. Entities that fail the filter are excluded from results

**Important**: The projection query ensures all needed fields are loaded from the database, regardless of what the GraphQL query selected. This is why projections are required - filters cannot rely on fields being available from the GraphQL query result.

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
* **Same performance**: Uses the same efficient projection query mechanism
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