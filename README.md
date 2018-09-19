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
  entities (ids: "1")
  {
    property
  }
}
```


##### Multiple

```graphql
{
  entities (ids: ["1", "2"])
  {
    property
  }
}
```


#### Where

Where statements are [and'ed](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-and-operator) together and executed in order.


##### Property Path

All where statements require a `path`. This is a full path to a, possible nested, property. Eg a property at the root level could be `Address`, while a nested property could be `Address.Street`. No null checking of nested values is done.


##### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte), [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime), [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), and [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64).


##### Supported Comparisons

 * `equal`
 * `notEqual`
 * `greaterThan`
 * `greaterThanOrEqual`
 * `lessThan`
 * `lessThanOrEqual`:
 * `contains`: Only works with `string`
 * `startsWith`: Only works with `string`
 * `endsWith`: Only works with `string`
 * `in`: Check if a member existing in a given collection of values
 * `like`: Performs a SQL Like by using `EF.Functions.Like`

Case of comparison names are ignored. So, for example, `EndsWith`, `endsWith`, and `endswith` are  allowed.


##### Single

Single where statements can be expressed:

```graphql
{
  entities
  (where: {path: "Property", comparison: "equal", value: "the value"})
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
      {path: "Property", comparison: "startsWith", value: "Valu"}
      {path: "Property", comparison: "endsWith", value: "ue"}
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
  (where: {path: "Property", comparison: "in", value: ["Value1", "Value2"]})
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
  (where: {path: "Property", comparison: "equal"})
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

The DbContext is only used to interrogate `DbContext.Model`, as such it only needs to be short lived. So the context can be cleaned up after calling `RegisterInContainer`:

```csharp
using (var dataContext = BuildDataContext())
{
    EfGraphQLConventions.RegisterInContainer(serviceCollection, dataContext)
}
```


### Connection Types

GraphQL enables paging via [Connections](https://graphql.org/learn/pagination/#complete-connection-model). When using Connections in GraphQL.net it is [necessary to register several types in the container](https://github.com/graphql-dotnet/graphql-dotnet/issues/451#issuecomment-335894433):

```csharp
services.AddTransient(typeof(ConnectionType<>));
services.AddTransient(typeof(EdgeType<>));
services.AddSingleton<PageInfoType>();
```

There is a helper methods to perform the above:

```csharp
EfGraphQLConventions.RegisterConnectionTypesInContainer(IServiceCollection services);
```

or

```csharp
EfGraphQLConventions.RegisterConnectionTypesInContainer(Action<Type> register)
```


### DependencyInjection and ASP.Net Core

As with GraphQL .net, GraphQL.EntityFramework makes no assumptions on the container or web framework it is hosted in. However given [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) and [ASP.Net Core](https://docs.microsoft.com/en-us/aspnet/core/) are the most likely usage scenarios, the below will address those scenarios explicitly.

See the GraphQL .net [documentation for ASP.Net Core](https://graphql-dotnet.github.io/docs/getting-started/dependency-injection#aspnet-core) and the [ASP.Net Core sample](https://github.com/graphql-dotnet/examples/tree/master/src/AspNetCoreCustom/Example).

The Entity Framework Data Context instance is generally [scoped per request](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes-and-registration-options). This can be done in the [Startup.ConfigureServices method](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup#the-configureservices-method):

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(provider => MyDataContextBuilder.BuildDataContext());
    }
}
```

Entity Framework also provides [several helper methods](https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#using-dbcontext-with-dependency-injection) to control a DataContexts lifecycle. For example:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDataContext>(provider => DataContextBuilder.BuildDataContext());
    }
}
```

See also [EntityFrameworkServiceCollectionExtensions](https://docs.microsoft.com/en-us/ef/core/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions)

With the DataContext existing in the container, it can be resolved in the controller that handles the GraphQL query:

```csharp

[Route("[controller]")]
[ApiController]
public class GraphQlController : Controller
{
    IDocumentExecuter executer;
    ISchema schema;

    public GraphQlController(ISchema schema, IDocumentExecuter executer)
    {
        this.schema = schema;
        this.executer = executer;
    }

    [HttpPost]
    public Task<ExecutionResult> Post(
        [BindRequired, FromBody] PostBody body,
        [FromServices] MyDataContext dataContext,
        CancellationToken cancellation)
    {
        return Execute(dataContext, body.Query, body.OperationName, body.Variables, cancellation);
    }

    public class PostBody
    {
        public string OperationName;
        public string Query;
        public JObject Variables;
    }

    [HttpGet]
    public Task<ExecutionResult> Get(
        [FromQuery] string query,
        [FromQuery] string variables,
        [FromQuery] string operationName,
        [FromServices] MyDataContext dataContext,
        CancellationToken cancellation)
    {
        var jObject = ParseVariables(variables);
        return Execute(dataContext, query, operationName, jObject, cancellation);
    }

    async Task<ExecutionResult> Execute(MyDataContext dataContext, string query, string operationName, JObject variables, CancellationToken cancellation)
    {
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            OperationName = operationName,
            Inputs = variables?.ToInputs(),
            UserContext = dataContext,
            CancellationToken = cancellation,
#if (DEBUG)
            ExposeExceptions = true,
            EnableMetrics = true,
#endif
        };

        var result = await executer.ExecuteAsync(executionOptions).ConfigureAwait(false);

        if (result.Errors?.Count > 0)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }

        return result;
    }

    static JObject ParseVariables(string variables)
    {
        if (variables == null)
        {
            return null;
        }

        try
        {
            return JObject.Parse(variables);
        }
        catch (Exception exception)
        {
            throw new Exception("Could not parse variables.", exception);
        }
    }
}
```

Note that the instance of the DataContext is passed to the [GraphQL .net User Context](https://graphql-dotnet.github.io/docs/getting-started/user-context).

The same instance of the DataContext can then be accessed in the `resolve` delegate by casting the `ResolveFieldContext.UserContext` to the DataContext type:

```csharp
public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) : base(efGraphQlService)
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
```


### Testing the GraphQlController

The `GraphQlController` can be tested using the [ASP.NET Integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests) via the [Microsoft.AspNetCore.Mvc.Testing NuGet package](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing).

```csharp
public class GraphQlControllerTests
{
    static HttpClient client;

    static GraphQlControllerTests()
    {
        var server = GetTestServer();
        client = server.CreateClient();
    }

    [Fact]
    public async Task Get()
    {
        var query = @"
{
  companies
  {
    id
  }
}";
        var response = await ExecuteGet(query);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
    }

    [Fact]
    public async Task Get_variable()
    {
        var query = @"
query ($id: String!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        var response = await ExecuteGet(query, variables);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
    }

    [Fact]
    public async Task Get_null_query()
    {
        var response = await ExecuteGet();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("GraphQL.ExecutionError: A query is required.", result);
    }

    static Task<HttpResponseMessage> ExecuteGet(string query = null, object variables = null)
    {
        var compressed = CompressQuery(query);
        var variablesString = ToJson(variables);
        var uri = $"graphql?query={compressed}&variables={variablesString}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task Post()
    {
        var response = await ExecutePost("{companies{id}}");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_variable()
    {
        var variables = new
        {
            id = "1"
        };
        var response = await ExecutePost("query ($id: String!){companies(ids:[$id]){id}}", variables);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_null_query()
    {
        var response = await ExecutePost();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("GraphQL.ExecutionError: A query is required.", result);
    }

    static Task<HttpResponseMessage> ExecutePost(string query = null, object variables = null)
    {
        query = CompressQuery(query);
        var body = new
        {
            query,
            variables
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = new StringContent(ToJson(body), Encoding.UTF8, "application/json")
        };
        return client.SendAsync(request);
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }

    static string ToJson(object target)
    {
        if (target == null)
        {
            return "";
        }
        return JsonConvert.SerializeObject(target);
    }

    static string CompressQuery(string query)
    {
        if (query == null)
        {
            return "";
        }
        return Compress.Query(query);
    }
}
```


## Defining Graphs


### Includes and Navigation properties.

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

```csharp
context.Heros
        .Include("Friends")
        .Include("Friends.Address");
```

The string for the include is taken from the field name when using `AddNavigationField` or `AddNavigationConnectionField` with the first character upper cased. This value can be overridden using the optional parameter `includeNames` .  Note that  `includeNames` is an `IEnumerable<string>` so that multiple navigation properties can optionally be included for a single node.


### Fields

Queries in GraphQL.net are defined using the [Fields API](https://graphql-dotnet.github.io/docs/getting-started/introduction#queries). Fields can be mapped to Entity Framework by using `IEfGraphQLService`. `IEfGraphQLService` can be used in either a root query or a nested query via dependency injection. Alternatively the base type `EfObjectGraphType` or `EfObjectGraphType<TSource>` can be used for root or nested graphs respectively. The below samples all use the base type approach as it results in slightly less code.


#### Root Query

```csharp
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
```


#### Typed Graph

```csharp
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

```csharp
public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddQueryConnectionField<CompanyGraph, Company>(
            name: "companies",
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


##### Response

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


#### Typed Graph

```csharp
public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        AddNavigationConnectionField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
    }
}
```

## Filters

Sometimes, in the context of constructing an EF query, it is not possible to know if any given item should be returned in the results. For example when performing authorization where the rules rules are pulled from a different system, and that information does not exist in the database.

`GlobalFilters` allows a custom function to be executed after the EF query execution and determine if any given node should be included in the result.

Notes:

 * When evaluated on nodes of a collection, excluded nodes will be replaced removed from collection.
 * When evaluated on a property node, the value will be replaced with null.
 * When doing paging or counts, there is currently no smarts that adjust counts or pages sizes when items are excluded. If this is required submit a PR that adds this feature, or don't mix filters with paging.
 * The filter is passed the current [User Context](https://graphql-dotnet.github.io/docs/getting-started/user-context) and the node item instance.
 * Filters will not be executed on null item instance.
 * A [Type.IsAssignableFrom](https://docs.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom) check will be performed to determine if an item instance should be filtered based on the `<TItem>`.
 * Filters are static and hence shared for the current [AppDomain](https://docs.microsoft.com/en-us/dotnet/api/system.appdomain).

#### Signature:

```
public delegate bool Filter<in TReturn>(object userContext, TReturn input);

GlobalFilters.Add<TItem>(Filter<TItem> filter);
```

#### Usage:

```
GlobalFilters.Add<Target>((userContext, target) => target.Property != "Ignore");
```

## Icon

<a href="https://thenounproject.com/term/database/1631008/" target="_blank">memory</a> designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)