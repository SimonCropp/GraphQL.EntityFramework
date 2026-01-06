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