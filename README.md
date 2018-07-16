# GraphQL.EntityFramework

Add [EntityFramework Core IQueryable](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.system-linq-iqueryable-provider) support to [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)


## NuGet [![NuGet Status](http://img.shields.io/nuget/v/GraphQL.EntityFramework.svg?longCache=true&style=flat)](https://www.nuget.org/packages/GraphQL.EntityFramework/)

https://nuget.org/packages/GraphQL.EntityFramework/

    PM> Install-Package GraphQL.EntityFramework


## Usage


### Arguments

The argumetns supported are `where`, `skip` and `take`.


#### Where


##### Property Path

All where statemnts requer a `path`. This is a full path to a, possible nested, property. Eg a property at the root level could be `Address`, while a nested property could be `Address.Street`.  No null checking of nested  values is done.


##### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte),  [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), and [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64).


##### Supported Comparions

 * `Equal`: alias `==`
 * `NotEqual`: alias `!=`
 * `GreaterThan`: alias `>`
 * `GreaterThanOrEqual`: alias `>=`
 * `LessThan`: alias `<`
 * `LessThanOrEqual`: alias `<=`
 * `Contains`: Only works with `string`
 * `StartsWith`: Only works with `string`
 * `EndsWith`: Only works with `string`
 * `In`: Check if a member existsing in a given collection of values.

Case of comparison names are ignored. So, for example, `EndsWith`, `endsWith`, and `endswith` are  allowed.


##### Single

Single where statements can be expressed:

```
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

```
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

```c#
{
  testEntities
  (where: {path: "Property", comparison: "In", value: ["Value1", "Value2"]})
  {
    property
  }
}
```

```
{
  entities
  (where: {path: "Property", comparison: "==", value: "the value"})
  {
    property
  }
}
```

Where statements are and'ed together and executed in order


#### Take

[Queryable.Take](https://msdn.microsoft.com/en-us/library/bb300906(v=vs.110).aspx) or
[Enumerable.Take](https://msdn.microsoft.com/en-us/library/bb503062.aspx) can be used as follows:

```
{
  entities (take: 1)
  {
    property
  }
}
```


#### Skip

[Queryable.Skip](https://msdn.microsoft.com/en-us/library/bb357513.aspx) or
[Enumerable.Skip](https://msdn.microsoft.com/en-us/library/bb358985.aspx) can be used as follows:

```
{
  entities (skip: 1)
  {
    property
  }
}
```


## Defining Graphs


### Fields


#### Root Query

```c#
public class Query : ObjectGraphType
{
    public Query()
    {
        this.AddQueryField<CompanyGraph, Company>(
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
    public CompanyGraph()
    {
        AddEnumerableField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
    }
}
```


### Connections


#### Root Query

```c#
public class Query : ObjectGraphType
{
    public Query()
    {
        this.AddQueryConnectionField<CompanyGraph, Company>(
            name: "companiesConnection",
            includeName: "Companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Companies;
            });
    }
}
```


#### Typed Graph

```c#
public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph()
    {
        AddEnumerableConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeName: "Employees");
    }
}
```


## Icon

<a href="https://thenounproject.com/term/database/1631008/" target="_blank">memory</a> designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)