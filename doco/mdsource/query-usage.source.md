
# Query Usage


## Arguments

The arguments supported are `ids`, `where`, `orderBy` , `skip`, and `take`.

Arguments are executed in that order.


### Ids

Queries entities by id. Currently the only supported identity member (property or field) name is `Id`.


#### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte), [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime), [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), and [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64).


#### Single

```graphql
{
  entities (ids: "1")
  {
    property
  }
}
```


#### Multiple

```graphql
{
  entities (ids: ["1", "2"])
  {
    property
  }
}
```


### Where

Where statements are [and'ed](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-and-operator) together and executed in order.


#### Property Path

All where statements require a `path`. This is a full path to a, possible nested, property. Eg a property at the root level could be `Address`, while a nested property could be `Address.Street`. No null checking of nested values is done.


#### Supported Types

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string), [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid), [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double), [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean), [Float](https://docs.microsoft.com/en-us/dotnet/api/system.float), [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte), [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime), [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset), [Decimal](https://docs.microsoft.com/en-us/dotnet/api/system.decimal), [Int16](https://docs.microsoft.com/en-us/dotnet/api/system.int16), [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32), [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64), [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16), [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32), [UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64), and [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum).


#### Supported Comparisons

 * `equal` (the default value if `comparison` is omitted)
 * `notEqual`
 * `greaterThan`
 * `greaterThanOrEqual`
 * `lessThan`
 * `lessThanOrEqual`:
 * `contains`: Only works with `string`
 * `startsWith`: Only works with `string`
 * `endsWith`: Only works with `string`
 * `in`: Check if a member existing in a given collection of values
 * `notIn`: Check if a member doesn't exist in a given collection of values
 * `like`: Performs a SQL Like by using `EF.Functions.Like`

Case of comparison names are ignored. So, for example, `EndsWith`, `endsWith`, and `endswith` are  allowed.


#### Single

Single where statements can be expressed:

```graphql
{
  entities
  (where: {
    path: "Property",
    comparison: "equal",
    value: "the value"})
  {
    property
  }
}
```


#### Multiple

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


#### Where In

```graphql
{
  testEntities
  (where: {
    path: "Property",
    comparison: "in",
    value: ["Value1", "Value2"]})
  {
    property
  }
}
```


#### Case Sensitivity

All string comparisons are, by default, done using no [StringComparison](https://msdn.microsoft.com/en-us/library/system.stringcomparison.aspx). A custom StringComparison can be used via the `case` attribute.

```graphql
{
  entities
  (where: {
    path: "Property",
    comparison: "endsWith",
    value: "the value",
    case: "Ordinal"})
  {
    property
  }
}
```


#### Null

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


### OrderBy


#### Ascending

```graphql
{
  entities (orderBy: {path: "Property"})
  {
    property
  }
}
```


#### Descending

```graphql
{
  entities (orderBy: {path: "Property", descending: true})
  {
    property
  }
}
```


### Take

[Queryable.Take](https://msdn.microsoft.com/en-us/library/bb300906(v=vs.110).aspx) or [Enumerable.Take](https://msdn.microsoft.com/en-us/library/bb503062.aspx) can be used as follows:

```graphql
{
  entities (take: 1)
  {
    property
  }
}
```


### Skip

[Queryable.Skip](https://msdn.microsoft.com/en-us/library/bb357513.aspx) or [Enumerable.Skip](https://msdn.microsoft.com/en-us/library/bb358985.aspx) can be used as follows:

```graphql
{
  entities (skip: 1)
  {
    property
  }
}
```