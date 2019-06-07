# Defining Graphs


## Includes and Navigation properties.

Entity Framework has the concept of [Navigation Properties](https://docs.microsoft.com/en-us/ef/core/modeling/relationships):

> A property defined on the principal and/or dependent entity that contains a reference(s) to the related entity(s).

In the context of GraphQL, Root Graph is the entry point to performing the initial EF query. Nested graphs then usually access navigation properties to return data, or perform a new EF query. New EF queries can be performed with `AddQueryField` and `AddQueryConnectionField`. Navigation properties queries are performed using `AddNavigationField` and `AddNavigationConnectionField`.

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


## Fields

Queries in GraphQL.net are defined using the [Fields API](https://graphql-dotnet.github.io/docs/getting-started/introduction#queries). Fields can be mapped to Entity Framework by using `IEfGraphQLService`. `IEfGraphQLService` can be used in either a root query or a nested query via dependency injection. Alternatively the base type `EfObjectGraphType` or `EfObjectGraphType<TSource>` can be used for root or nested graphs respectively. The below samples all use the base type approach as it results in slightly less code.


### Root Query

snippet: rootQuery

`AddQueryField` will result in all matching being found and returned.

`AddSingleField` will result in a single matching being found and returned. This approach uses [`IQueryable<T>.SingleOrDefaultAsync`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.entityframeworkqueryableextensions.singleordefaultasync) as such, if no records are found a null will be returned, and if multiple records match then an exception will be thrown.


### Typed Graph

snippet: typedGraph


## Connections


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


## Manually Apply `WhereExpression`

In some cases, you may want to use `Field` instead of `AddQueryField`/`AddSingleField`/etc but still would like to use apply the `where` argument. This can be useful when the returned `Graph` type is not for an entity (for example, aggregate results). To support this, you must:

 * Add the `WhereExpressionGraph` argument
 * Apply the `where` argument expression using `ExpressionBuilder<T>.BuildPredicate(whereExpression)`

snippet: ManuallyApplyWhere