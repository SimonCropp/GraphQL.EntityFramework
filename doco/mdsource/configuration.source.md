# Configuration

toc


## Container Registration

Enabling is done via registering in a container.

The container registration can be done via adding to a [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection):

snippet: RegisterInContainer


### Inputs


#### IModel

Configuration requires an instance of `Microsoft.EntityFrameworkCore.Metadata.IModel`. This can be passed in as a parameter, or left as null to be resolved from the container. When `IModel` is resolved from the container, `IServiceProvider.GetService` is called first on `IModel`, then on `TDbContext`. If both return null, then an exception will be thrown.

To build an instance of an `IModel` at configuration time it can be helpful to have a class specifically for that purpose:

snippet: ModelBuilder


#### Resolve DbContext

A delegate that resolves the DbContext.

snippet: ResolveDbContext.cs

It has access to the current GraphQL user context.

If null then the DbContext will be resolved from the container.


#### Resolve Filters

A delegate that resolves the [Filters](filters.md).

snippet: ResolveFilters.cs

It has access to the current GraphQL user context.

If null then the Filters will be resolved from the container.


### Usage

snippet: RegisterInContainer

Then the `IEfGraphQLService` can be resolved via [dependency injection in GraphQL.net](https://graphql-dotnet.github.io/docs/guides/advanced#dependency-injection) to be used in `ObjectGraphType`s when adding query fields.


## DocumentExecuter

The default GraphQL `DocumentExecuter` uses [Task.WhenAll](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) to resolve async fields. This can result in multiple EF queries being executed on different threads and being resolved out of order. In this scenario the following exception will be thrown.

> Message: System.InvalidOperationException : A second operation started on this context before a previous operation completed. This is usually caused by different threads using the same instance of DbContext, however instance members are not guaranteed to be thread safe. This could also be caused by a nested query being evaluated on the client, if this is the case rewrite the query avoiding nested invocations.

To avoid this a custom implementation of `DocumentExecuter` but be used that uses `SerialExecutionStrategy` when the operation type is `OperationType.Query`. There is one included in this library named `EfDocumentExecuter`:

snippet: EfDocumentExecuter.cs


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
          provider => MyDbContextBuilder.BuildDbContext());
    }
}
```

Entity Framework also provides [several helper methods](https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#using-dbcontext-with-dependency-injection) to control a DbContexts lifecycle. For example:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(
          provider => DbContextBuilder.BuildDbContext());
    }
}
```

See also [EntityFrameworkServiceCollectionExtensions](https://docs.microsoft.com/en-us/ef/core/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions)

With the DbContext existing in the container, it can be resolved in the controller that handles the GraphQL query:

snippet: GraphQlController


## Multiple DbContexts

Multiple different DbContext types can be registered and used.


### UserContext

A user context that exposes both types.

snippet: MultiUserContext


### Register in container

Register both DbContext types in the container and include how those instance can be extracted from the GraphQL context:

snippet: RegisterMultipleInContainer


### ExecutionOptions

Use the user type to pass in both DbContext instances.


snippet: MultiExecutionOptions


### Query

Use both DbContexts in a Query:

snippet: MultiContextQuery.cs


### GraphType

Use a DbContext in a Graph:

snippet: Entity1Graph.cs


## Testing the GraphQlController

The `GraphQlController` can be tested using the [ASP.NET Integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests) via the [Microsoft.AspNetCore.Mvc.Testing NuGet package](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing).

snippet: GraphQlControllerTests


## GraphQlExtensions

The `GraphQlExtensions` class exposes some helper methods:


### ExecuteWithErrorCheck

Wraps the `DocumentExecuter.ExecuteAsync` to throw if there are any errors.

snippet: ExecuteWithErrorCheck