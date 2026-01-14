# Defining Graphs


## Includes and Navigation properties.

Entity Framework has the concept of [Navigation Properties](https://docs.microsoft.com/en-us/ef/core/modeling/relationships):

> A property defined on the principal and/or dependent entity that contains a reference(s) to the related entity(s).

In the context of GraphQL, Root Graph is the entry point to performing the initial EF query. Nested graphs then usually access navigation properties to return data, or perform a new EF query. New EF queries can be performed with `AddQueryField` and `AddQueryConnectionField`. Navigation properties queries are performed using `AddNavigationField` and `AddNavigationConnectionField`. For the above `*ConnectionField` refer to the GraphQL concept of pagination using [Connections](https://graphql.org/learn/pagination/).

When performing a query there are several approaches to [Loading Related Data](https://docs.microsoft.com/en-us/ef/core/querying/related-data)

 * **Eager loading** means that the related data is loaded from the database as part of the initial query.
 * **Explicit loading** means that the related data is explicitly loaded from the database at a later time.
 * **Lazy loading** means that the related data is transparently loaded from the database when the navigation property is accessed.

Ideally, all navigation properties would be eagerly loaded as part of the root query. However determining what navigation properties to eagerly is difficult in the context of GraphQL. The reason is, given the returned hierarchy of data is dynamically defined by the requesting client, the root query cannot know what properties to include. To work around this GraphQL.EntityFramework interrogates the incoming query to derive the includes. So for example take the following query

```graphql
{
  hero {
    name
    friends {
      name
      address {
        town
      }
    }
  }
}
```

Would result in the following query being performed

```cs
context.Heros
        .Include("Friends")
        .Include("Friends.Address");
```

The string for the include is taken from the field name when using `AddNavigationField` or `AddNavigationConnectionField` with the first character upper cased. This value can be overridden using the optional parameter `includeNames` . Note that `includeNames` is an `IEnumerable<string>` so that multiple navigation properties can optionally be included for a single node.


## Projections

GraphQL.EntityFramework automatically optimizes Entity Framework queries by using projections. When a GraphQL query is executed, only the fields explicitly requested in the query are loaded from the database, rather than loading entire entity objects.


### How Projections Work

When querying entities through GraphQL, the incoming query is analyzed and a projection expression is built that includes:

1. **Primary Keys** - Always included (e.g., `Id`)
2. **Foreign Keys** - Always included automatically (e.g., `ParentId`, `CategoryId`)
3. **Requested Scalar Fields** - Fields explicitly requested in the GraphQL query
4. **Navigation Properties** - With their own nested projections

For example, given this entity:

snippet: ProjectionEntity

And this GraphQL query:

```graphql
{
  order(id: "1") {
    id
    orderNumber
  }
}
```

The library will generate an EF query that projects to:

snippet: ProjectionExpression

Note that `TotalAmount` and `InternalNotes` are **not** loaded from the database since they weren't requested.


### Foreign Keys in Custom Resolvers

The automatic inclusion of foreign keys is useful when writing custom field resolvers. Since foreign keys are always available in the projected entity, it is safe to use them without worrying about whether they were explicitly requested:

snippet: ProjectionCustomResolver

Without automatic foreign key inclusion, `context.Source.CustomerId` would be `0` (or `Guid.Empty` for Guid keys) if `customerId` wasn't explicitly requested in the GraphQL query, causing the query to fail.


### When Projections Are Not Used

Projections are bypassed and the full entity is loaded in these cases:

1. **Read-only computed properties** - When any property has no setter or is expression-bodied (at any level, including nested navigations)
2. **Abstract entity types** - When the root entity type or any navigation property type is abstract

In these cases, the query falls back to loading the complete entity with all its properties.


### Performance Benefits

Projections provide significant performance improvements:

- **Reduced database load** - Only requested columns are retrieved from the database
- **Less data transferred** - Smaller result sets from database to application
- **Lower memory usage** - Smaller objects in memory

For queries that request only a few fields from entities with many properties, the performance improvement can be substantial.


## Fields

Queries in GraphQL.net are defined using the [Fields API](https://graphql-dotnet.github.io/docs/getting-started/introduction#queries). Fields can be mapped to Entity Framework by using `IEfGraphQLService`. `IEfGraphQLService` can be used in either a root query or a nested query via dependency injection. Alternatively convenience methods are exposed on the types `EfObjectGraphType` or `EfObjectGraphType<TSource>` for root or nested graphs respectively. The below samples all use the base type approach as it results in slightly less code.


### Root Query

snippet: rootQuery

`AddQueryField` will result in all matching being found and returned.

`AddSingleField` will result in a single matching being found and returned. This approach uses [`IQueryable<T>.SingleOrDefaultAsync`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.entityframeworkqueryableextensions.singleordefaultasync) as such, if no records are found a null will be returned, and if multiple records match then an exception will be thrown.


### Typed Graph

snippet: typedGraph


## Projected Fields

The ProjectedField API provides a way to explicitly project and transform entity properties in GraphQL fields. Use cases include:

- Transform scalar properties (e.g., converting to uppercase, formatting dates)
- Apply context-aware transformations (e.g., filtering based on user permissions)
- Perform async operations on projected data
- Ensure specific properties are loaded from the database

**Note:** The Roslyn analyzer (GQLEF001) will warn when accessing `context.Source.PropertyName` directly, suggesting use of ProjectedField methods instead.


### Understanding the Two Parameters

ProjectedField methods accept two key parameters that work together to safely access entity properties:

**1. `projection`** - Specifies the complete path to the data
- An Expression showing the full navigation path from `source` to the target data
- For navigation fields: `source => source.Property` or `source => source.Navigation.Property`
- For list fields: Uses a `navigation` expression to get the collection, then a `projection` for each item
- Automatically detects and includes all navigation properties in the path
- Gets compiled once at registration time for efficiency

**2. `transform`** - Transforms the projected data into the final GraphQL field value
- Receives the projected data (not the full entity)
- Can perform calculations, formatting, async operations, etc.
- Can optionally access the GraphQL context for context-aware transformations

**Execution flow:**

```csharp
// 1. PROJECTION - Extract needed data from source (navigations auto-included)
var projectedData = compiledProjection(context.Source);  // e.g., extracts source.Property

// 2. Apply filters (if any)
if (!ShouldInclude(context.Source)) return default;

// 3. TRANSFORM - Create final value
var result = await transform(fieldContext, projectedData);  // e.g., ToUpper()
return result;
```

This approach ensures that all required navigation properties are automatically eager-loaded from the database before the transform runs, solving the problem where `context.Source.PropertyName` may be null if not included in the GraphQL query projection.


### Basic Transform

snippet: ProjectedFieldBasicTransform


### Async Transform

snippet: ProjectedFieldAsyncTransform


### Context-Aware Transform

snippet: ProjectedFieldContextAwareTransform


### List Field

snippet: ProjectedFieldListField


### Nested Navigation

snippet: ProjectedFieldNestedNavigation

**Automatic Include Detection:**

The projection expression automatically detects and includes all navigation properties in the path. No manual specification is required.

In the example above:
- `projection: source => source.Level2Entity.Level3Entity.Property`
- Automatically detects: `Level2Entity` and `Level3Entity`
- Automatically adds includes: `["Level2Entity", "Level2Entity.Level3Entity"]`
- EF Core eager-loads both navigations before the projection executes

This automatic detection ensures all required navigation properties are eager-loaded without any manual configuration.


## Connections

Creating a page-able field is supported through [GraphQL Connections](https://graphql.org/learn/pagination/) by calling `IEfGraphQLService.AddNavigationConnectionField` (for an EF navigation property), or `IEfGraphQLService.AddQueryConnectionField` (for an IQueryable). Alternatively convenience methods are exposed on the types `EfObjectGraphType` or `EfObjectGraphType<TSource>` for root or nested graphs respectively.


### Root Query


#### Graph Type

snippet: ConnectionRootQuery


#### Request

```graphql
{
  companies(first: 2, after: "1") {
    totalCount
    edges {
      node {
        id
        content
        employees {
          id
          content
        }
      }
      cursor
    }
    pageInfo {
      startCursor
      endCursor
      hasPreviousPage
      hasNextPage
    }
  }
}
```


#### Response

```js
{
  "data": {
    "companies": {
      "totalCount": 4,
      "edges": [
        {
          "node": {
            "id": "1",
            "content": "Company1",
            "employees": [
              {
                "id": "2",
                "content": "Employee1"
              },
              {
                "id": "3",
                "content": "Employee2"
              }
            ]
          },
          "cursor": "1"
        },
        {
          "node": {
            "id": "4",
            "content": "Company3",
            "employees": []
          },
          "cursor": "2"
        }
      ],
      "pageInfo": {
        "startCursor": "1",
        "endCursor": "2",
        "hasPreviousPage": true,
        "hasNextPage": true
      }
    }
  }
}
```


### Typed Graph

snippet: ConnectionTypedGraph


## Enums

```cs
public class DayOfTheWeekGraph : EnumerationGraphType<DayOfTheWeek>
{
}
```

```cs
public class ExampleGraph : ObjectGraphType<Example>
{
    public ExampleGraph()
    {
        Field(x => x.DayOfTheWeek, type: typeof(DayOfTheWeekGraph));
    }
}
```

 * [GraphQL .NET - Schema Types / Enumerations](https://graphql-dotnet.github.io/docs/getting-started/schema-types/#enumerations)


## AutoMap

`Mapper.AutoMap` can be used to remove repetitive code by mapping all properties of a type.

For example for this graph:

```cs
public class EmployeeGraph :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "company",
            resolve: context => context.Source.Company);
        Field(employee => employee.Age);
        Field(employee => employee.Content);
        Field(employee => employee.CompanyId);
        Field(employee => employee.Id);
    }
}
```

The equivalent graph using AutoMap is:

```cs
public class EmployeeGraph :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}
```

The underlying behavior of AutoMap is:

 * Calls `IEfGraphQLService{TDbContext}.AddNavigationField{TSource,TReturn}` for all non-list EF navigation properties.
 * Calls `IEfGraphQLService{TDbContext}.AddNavigationListField{TSource,TReturn}` for all EF navigation properties.
 * Calls `ComplexGraphType{TSourceType}.AddField` for all other properties

An optional list of `exclusions` can be passed to exclude a subset of properties from mapping.

`Mapper.AddIgnoredType` can be used to exclude properties (of a certain type) from mapping.


## Manually Apply `WhereExpression`

In some cases, it may be necessary to use `Field` instead of `AddQueryField`/`AddSingleField`/etc but still would like to use apply the `where` argument. This can be useful when the returned `Graph` type is not for an entity (for example, aggregate results). To support this:

 * Add the `WhereExpressionGraph` argument
 * Apply the `where` argument expression using `ExpressionBuilder<T>.BuildPredicate(whereExpression)`

snippet: ManuallyApplyWhere


## Resolving DbContext

Sometimes it is necessary to access the current DbContext from withing the base `QueryGraphType.Field` method. in this case the custom `ResolveEfFieldContext` is not available. In this scenario `QueryGraphType.ResolveDbContext` can be used to resolve the current DbContext.

snippet: QueryResolveDbContext


## ArgumentProcessor

`ArgumentProcessor` (via the method `ApplyGraphQlArguments`) is responsible for extracting the various parts of the [GraphQL query argument](query-usage.md) and applying them to an `IQueryable<T>`. So, for example, each [where argument](query-usage.md#where) is mapped to a [IQueryable.Where](https://docs.microsoft.com/en-us/dotnet/api/system.linq.queryable.where) and each [skip argument](query-usage.md#skip) is mapped to a [IQueryable.Where](https://docs.microsoft.com/en-us/dotnet/api/system.linq.queryable.skip). 

The arguments are parsed and mapped each time a query is executer.

ArgumentProcessor is generally considered an internal API and not for public use. However there are some advanced scenarios, for example when building subscriptions, that ArgumentProcessor is useful.
