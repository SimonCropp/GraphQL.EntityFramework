# GraphQL.EntityFramework



## NuGet [![NuGet Status](http://img.shields.io/nuget/v/GraphQL.EntityFramework.svg?longCache=true&style=flat)](https://www.nuget.org/packages/GraphQL.EntityFramework/)

https://nuget.org/packages/GraphQL.EntityFramework/

    PM> Install-Package GraphQL.EntityFramework


## Usage


### Arguments

#### Where

##### Supported Types

 * [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)
 * [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)
 * [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)
 * [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)
 * [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float)
 * [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte)
 * [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16)
 * [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)
 * [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64)
 * [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)
 * [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32)
 * [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64)

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
 * `In`

##### Single

```
{
  entities 
  (where: {path: 'Property', comparison: '==', value: 'the value'})
  {
    property
  }
}
```

##### Multiple

```
{
  testEntities
  (where:
    [
      {path: 'Property', comparison: 'startsWith"", value: 'Valu'}
      {path: 'Property', comparison: 'endsWith"", value: 'ue'}
    ]
  )
  {
    property
  }
}
```

#####



#### Take

#### Skip


The main entry point is `InMemoryContextBuilder` which can be used to build an in-memory context.



###

## Icon

<a href="https://thenounproject.com/term/20database/1631008/" target="_blank">memory</a> designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)