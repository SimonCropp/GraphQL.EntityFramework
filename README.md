# GraphQL.EntityFramework

Add [EntityFramework Core IQueryable](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.system-linq-iqueryable-provider) support to [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)


## NuGet [![NuGet Status](http://img.shields.io/nuget/v/GraphQL.EntityFramework.svg?longCache=true&style=flat)](https://www.nuget.org/packages/GraphQL.EntityFramework/)

https://nuget.org/packages/GraphQL.EntityFramework/

    PM> Install-Package GraphQL.EntityFramework


## Query Usage


### Arguments

The arguments supported are `ids`, `where`, `orderBy` , `skip`, and `take`.

Arguments are executed in that order.


#### Ids

Queries entities by id. Currently the only supported identity member (property or field) name is `Id`.


##### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte), [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime), [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), and [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64).


##### Single

```graphql
{
  entities (ids: "00000000-0000-0000-0000-000000000001")
  {
    property
  }
}
```


##### Multiple

```graphql
{
  entities (ids: ["00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002"])
  {
    property
  }
}
```


#### Where

Where statements are [and'ed](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-and-operator) together and executed in order.


##### Property Path

All where statements require a `path`. This is a full path to a, possible nested, property. Eg a property at the root level could be `Address`, while a nested property could be `Address.Street`.  No null checking of nested  values is done.


##### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte), [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime), [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), and [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64).


##### Supported Comparisons

 * `Equal`: alias `==`
 * `NotEqual`: alias `!=`
 * `GreaterThan`: alias `>`
 * `GreaterThanOrEqual`: alias `>=`
 * `LessThan`: alias `<`
 * `LessThanOrEqual`: alias `<=`
 * `Contains`: Only works with `string`
 * `StartsWith`: Only works with `string`
 * `EndsWith`: Only works with `string`
 * `In`: Check if a member existing in a given collection of values.`
* `Like`: Performs a SQL Like by using `EF.Functions.Like`.

Case of comparison names are ignored. So, for example, `EndsWith`, `endsWith`, and `endswith` are  allowed.


##### Single

Single where statements can be expressed:

```graphql
{
  entities
  (where: {path: "Property", comparison: "==", value: "the value"})
  {
    property
  }
}
```


##### Multiple

Multiple where statements can be expressed:

```graphql
{
  entities
  (where:
    [
      {path: "Property", comparison: "startsWith"", value: "Valu"}
      {path: "Property", comparison: "endsWith"", value: "ue"}
    ]
  )
  {
    property
  }
}
```


##### Where In

```graphql
{
  testEntities
  (where: {path: "Property", comparison: "In", value: ["Value1", "Value2"]})
  {
    property
  }
}
```


##### Case Sensitivity

All string comparisons are, by default, done using no [StringComparison](https://msdn.microsoft.com/en-us/library/system.stringcomparison.aspx). A custom StringComparison can be used via the `case` attribute.

```graphql
{
  entities
  (where: {path: "Property", comparison: "endsWith", value: "the value", case: "Ordinal"})
  {
    property
  }
}
```

**Note that many [Database Providers](https://docs.microsoft.com/en-us/ef/core/providers/), including [SQL Server](https://docs.microsoft.com/en-us/ef/core/providers/sql-server/index), cannot correctly convert a case insensitive comparison to a server side query.** Hence this will result in the query being [resolved client side](https://docs.microsoft.com/en-us/ef/core/querying/client-eval#client-evaluation). If this is a concern it is recommended to [Disabling client evaluation](https://docs.microsoft.com/en-us/ef/core/querying/client-eval#disabling-client-evaluation).

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
}
```



##### Null

Null can be expressed by omitting the `value`:

```graphql
{
  entities
  (where: {path: "Property", comparison: "=="})
  {
    property
  }
}
```


#### OrderBy


##### Ascending

```graphql
{
  entities (orderBy: {path: "Property"})
  {
    property
  }
}
```


##### Descending

```graphql
{
  entities (orderBy: {path: "Property", descending: true})
  {
    property
  }
}
```


#### Take

[Queryable.Take](https://msdn.microsoft.com/en-us/library/bb300906(v=vs.110).aspx) or [Enumerable.Take](https://msdn.microsoft.com/en-us/library/bb503062.aspx) can be used as follows:

```graphql
{
  entities (take: 1)
  {
    property
  }
}
```


#### Skip

[Queryable.Skip](https://msdn.microsoft.com/en-us/library/bb357513.aspx) or [Enumerable.Skip](https://msdn.microsoft.com/en-us/library/bb358985.aspx) can be used as follows:

```graphql
{
  entities (skip: 1)
  {
    property
  }
}
```

## Configuration

Enabling is done via registering in a container.

This can be applied to an [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection):

```csharp
EfGraphQLConventions.RegisterInContainer(IServiceCollection services, DbContext dataContext);
```

Or via a delegate.

```csharp
EfGraphQLConventions.RegisterInContainer(Action<Type, object> registerInstance, DbContext dbContext)
```

Then the usage entry point `IEfGraphQLService` can be resolved via [dependency injection in GraphQL.net](https://graphql-dotnet.github.io/docs/guides/advanced#dependency-injection) to be used in `ObjectGraphType`s when adding query fields.

### Connection Types

GraphQL enables paging via [Connections](https://graphql.org/learn/pagination/#complete-connection-model). When using Connections in GraphQL.net it is [necessary to register several types in the container](https://github.com/graphql-dotnet/graphql-dotnet/issues/451#issuecomment-335894433):

```csharp
services.AddTransient(typeof(ConnectionType<>));
services.AddTransient(typeof(EdgeType<>));
services.AddSingleton<PageInfoType>();
```

There is a helper methods to perform the above:

``` csharp
EfGraphQLConventions.RegisterConnectionTypesInContainer(IServiceCollection services);
```

or 

```csharp
EfGraphQLConventions.RegisterConnectionTypesInContainer(Action<Type> register)
```



## Defining Graphs


### Includes and Navigation properties.

Entity Framework has the concept of [Navigation Properties](https://docs.microsoft.com/en-us/ef/core/modeling/relationships):

> A property defined on the principal and/or dependent entity that contains a reference(s) to the related entity(s). 

In the context of GraphQL, Root Graph is the entry point to performing the initial EF query. Nested graphs then usually access navigation properties to return data, or perform a new EF query. New EF queries can be performed with `AddQueryField` and `AddQueryConnectionField`. Navigation properties queries are performed using `AddNavigationField` and `AddNavigationConnectionField`.

When performing a query there are several approaches to [Loading Related Data](https://docs.microsoft.com/en-us/ef/core/querying/related-data)

- **Eager loading** means that the related data is loaded from the database as part of the initial query.
- **Explicit loading** means that the related data is explicitly loaded from the database at a later time.
- **Lazy loading** means that the related data is transparently loaded from the database when the navigation property is accessed.

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

```csharp
context.Heros
        .Include("Friends")
        .Include("Friends.Address");
```

The string for the include is taken from the field name when using `AddNavigationField` or `AddNavigationConnectionField` with the first character upper cased. This value can be overridden using the optional parameter `includeNames` .  Note that  `includeNames` is an `IEnumerable<string>` so that multiple navigation properties can optionally be included for a single node.




### Fields

Queries in GraphQL.net are defined using the [Fields API](https://graphql-dotnet.github.io/docs/getting-started/introduction#queries). Fields can be mapped to Entity Framework by using `IEfGraphQLService`. `IEfGraphQLService` can be used in either a root query or a nested query via dependency injection. Alternatively the base type `EfObjectGraphType` or `EfObjectGraphType<TSource>` can be used for root or nested graphs respectively. The below samples all use the base type approach as it results in slightly less code.

#### Root Query

```c#
public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });
        }
    }
}
```


#### Typed Graph

```c#
public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddNavigationField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
    }
}
```


### Connections


#### Root Query


##### Graph Type

```c#
public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddQueryConnectionField<CompanyGraph, Company>(
            name: "companiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Companies;
            });
    }
}
```


##### Request

```graphql
{
  companiesConnection(first: 2, after: "1") {
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


##### Response

```js
{
  "data": {
    "companiesConnection": {
      "totalCount": 4,
      "edges": [
        {
          "node": {
            "id": "bd042865-db68-4cdb-ad6c-b67cd3ad9602",
            "content": "Company1",
            "employees": [
              {
                "id": "b373d722-0ca5-4616-b074-d862367d1405",
                "content": "Employee1"
              },
              {
                "id": "66e4dbed-aeb4-4ba2-87db-e10d0d17255c",
                "content": "Employee2"
              }
            ]
          },
          "cursor": "1"
        },
        {
          "node": {
            "id": "d663e248-9d9f-4327-88da-7fd8f9e3fedb",
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

#### Typed Graph

```c#
public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddNavigationConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeName: "Employees");
    }
}
```





## Icon

<a href="https://thenounproject.com/term/database/1631008/" target="_blank">memory</a> designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)