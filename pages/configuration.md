<!--
This file was generate by MarkdownSnippets.
Source File: \pages\configuration.source.md
To change this file edit the source file and then re-run the generation using either the dotnet global tool (https://github.com/SimonCropp/MarkdownSnippets#githubmarkdownsnippets) or using the api (https://github.com/SimonCropp/MarkdownSnippets#running-as-a-unit-test).
-->


# Configuration

Enabling is done via registering in a container.

This can be applied to a [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection):

```csharp
EfGraphQLConventions.RegisterInContainer(IServiceCollection services, DbContext context);
```

Or via a delegate.

```csharp
EfGraphQLConventions.RegisterInContainer(Action<Type, object> register, DbContext context)
```

Then the usage entry point `IEfGraphQLService` can be resolved via [dependency injection in GraphQL.net](https://graphql-dotnet.github.io/docs/guides/advanced#dependency-injection) to be used in `ObjectGraphType`s when adding query fields.

The DbContext is only used to interrogate `DbContext.Model`, as such it only needs to be short lived. So the context can be cleaned up after calling `RegisterInContainer`:

```csharp
using (var context = BuildDataContext())
{
    EfGraphQLConventions.RegisterInContainer(serviceCollection, context)
}
```


## DocumentExecuter

The default GraphQL `DocumentExecuter` uses [Task.WhenAll](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) to resolve async fields. This can result in multiple EF queries being executed on different threads and being resolved out of order. In this scenario the following exception will be thrown.

> Message: System.InvalidOperationException : A second operation started on this context before a previous operation completed. This is usually caused by different threads using the same instance of DbContext, however instance members are not guaranteed to be thread safe. This could also be caused by a nested query being evaluated on the client, if this is the case rewrite the query avoiding nested invocations.

To avoid this a custom implementation of `DocumentExecuter` but be used that uses `SerialExecutionStrategy` when the operation type is `OperationType.Query`. There is one included in this library named `EfDocumentExecuter`:

<!-- snippet: EfDocumentExecuter.cs -->
```cs
using GraphQL.Execution;
using GraphQL.Language.AST;

namespace GraphQL.EntityFramework
{
    public class EfDocumentExecuter : DocumentExecuter
    {
        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            if (context.Operation.OperationType == OperationType.Query)
            {
                return new SerialExecutionStrategy();
            }
            return base.SelectExecutionStrategy(context);
        }
    }
}
```
<sup>[snippet source](/src/GraphQL.EntityFramework/EfDocumentExecuter.cs#L1-L18)</sup>
<!-- endsnippet -->


## Connection Types

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


## DependencyInjection and ASP.Net Core

As with GraphQL .net, GraphQL.EntityFramework makes no assumptions on the container or web framework it is hosted in. However given [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) and [ASP.Net Core](https://docs.microsoft.com/en-us/aspnet/core/) are the most likely usage scenarios, the below will address those scenarios explicitly.

See the GraphQL .net [documentation for ASP.Net Core](https://graphql-dotnet.github.io/docs/getting-started/dependency-injection#aspnet-core) and the [ASP.Net Core sample](https://github.com/graphql-dotnet/examples/tree/master/src/AspNetCoreCustom/Example).

The Entity Framework Data Context instance is generally [scoped per request](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes-and-registration-options). This can be done in the [Startup.ConfigureServices method](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup#the-configureservices-method):

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(
          provider => MyDataContextBuilder.BuildDataContext());
    }
}
```

Entity Framework also provides [several helper methods](https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#using-dbcontext-with-dependency-injection) to control a DataContexts lifecycle. For example:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDataContext>(
          provider => DataContextBuilder.BuildDataContext());
    }
}
```

See also [EntityFrameworkServiceCollectionExtensions](https://docs.microsoft.com/en-us/ef/core/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions)

With the DataContext existing in the container, it can be resolved in the controller that handles the GraphQL query:

<!-- snippet: GraphQlController -->
```cs
[Route("[controller]")]
[ApiController]
public class GraphQlController :
    Controller
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

    async Task<ExecutionResult> Execute(
        MyDataContext dataContext,
        string query,
        string operationName,
        JObject variables,
        CancellationToken cancellation)
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

        var result = await executer.ExecuteAsync(executionOptions)
            .ConfigureAwait(false);

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
<sup>[snippet source](/src/SampleWeb/GraphQlController.cs#L11-L103)</sup>
<!-- endsnippet -->

Note that the instance of the DataContext is passed to the [GraphQL .net User Context](https://graphql-dotnet.github.io/docs/getting-started/user-context).

The same instance of the DataContext can then be accessed in the `resolve` delegate by casting the `ResolveFieldContext.UserContext` to the DataContext type:

<!-- snippet: QueryUsedInController -->
```cs
public class Query :
    EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });
```
<sup>[snippet source](/src/SampleWeb/Query.cs#L5-L21)</sup>
<!-- endsnippet -->


## Testing the GraphQlController

The `GraphQlController` can be tested using the [ASP.NET Integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests) via the [Microsoft.AspNetCore.Mvc.Testing NuGet package](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing).

<!-- snippet: GraphQlControllerTests -->
```cs
public class GraphQlControllerTests
{
    static HttpClient client;

    static WebSocketClient websocketClient;

    static GraphQlControllerTests()
    {
        var server = GetTestServer();
        client = server.CreateClient();
        websocketClient = server.CreateWebSocketClient();
        websocketClient.ConfigureRequest =
            request =>
            {
                request.Headers["Sec-WebSocket-Protocol"] = "graphql-ws";
            };
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
        var response = await ClientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains(
            "{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}",
            result);
    }

    [Fact]
    public async Task Get_single()
    {
        var query = @"
query ($id: String!)
{
  company(id:$id)
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        var response = await ClientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains(@"{""data"":{""company"":{""id"":1}}}", result);
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

        var response = await ClientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("{\"companies\":[{\"id\":1}]}", result);
    }

    [Fact]
    public async Task Get_companies_paging()
    {
        var after = 1;
        var query = @"
query {
  companiesConnection(first:2, after:""" + after + @""") {
    edges {
      cursor
      node {
        id
      }
    }
    pageInfo {
      endCursor
      hasNextPage
    }
  }
}";
        var response = await ClientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());

        var page = result.SelectToken("..data..companiesConnection..edges[0].cursor")
            .Value<string>();
        Assert.NotEqual(after.ToString(), page);
    }

    [Fact]
    public async Task Post()
    {
        var query = @"
{
  companies
  {
    id
  }
}";
        var response = await ClientQueryExecutor.ExecutePost(client, query);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains(
            "{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}",
            result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_variable()
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
        var response = await ClientQueryExecutor.ExecutePost(client, query, variables);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("{\"companies\":[{\"id\":1}]}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public Task Should_subscribe_to_companies()
    {
        var resetEvent = new AutoResetEvent(false);

        var result = new GraphQLHttpSubscriptionResult(
            new Uri("http://example.com/graphql"),
            new GraphQLRequest
            {
                Query = @"
subscription {
  companyChanged {
    id
  }
}"
            },
            websocketClient);

        result.OnReceive += res =>
        {
            if (res != null)
            {
                Assert.Null(res.Errors);

                if (res.Data != null)
                {
                    resetEvent.Set();
                }
            }
        };

        var taskCancellationSource = new CancellationTokenSource();

        var task = result.StartAsync(taskCancellationSource.Token);

        Assert.True(resetEvent.WaitOne(TimeSpan.FromSeconds(10)));

        taskCancellationSource.Cancel();

        return task;
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }
}
```
<sup>[snippet source](/src/SampleWeb.Tests/GraphQlControllerTests.cs#L12-L211)</sup>
<!-- endsnippet -->


## GraphQlExtensions

The `GraphQlExtensions` class exposes some helper methods:


### ExecuteWithErrorCheck

Wraps the `DocumentExecuter.ExecuteAsync` to throw if there are any errors.

<!-- snippet: ExecuteWithErrorCheck -->
```cs
public static async Task<ExecutionResult> ExecuteWithErrorCheck(this IDocumentExecuter documentExecuter, ExecutionOptions executionOptions)
{
    Guard.AgainstNull(nameof(documentExecuter), documentExecuter);
    Guard.AgainstNull(nameof(executionOptions), executionOptions);
    var executionResult = await documentExecuter.ExecuteAsync(executionOptions)
        .ConfigureAwait(false);

    var errors = executionResult.Errors;
    if (errors != null && errors.Count > 0)
    {
        if (errors.Count == 1)
        {
            throw errors.First();
        }

        throw new AggregateException(errors);
    }

    return executionResult;
}
```
<sup>[snippet source](/src/GraphQL.EntityFramework/GraphQlExtensions.cs#L9-L32)</sup>
<!-- endsnippet -->
