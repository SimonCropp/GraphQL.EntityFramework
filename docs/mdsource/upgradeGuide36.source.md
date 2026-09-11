# Upgrading to 36.0.0

Version 36 has one breaking change.

 * The `orderBy` argument is an enum generated per entity rather than an input object. Breaking for every query that passes it.


## Enum orderBy

The `orderBy` argument keeps its type name, `{Type}OrderBy`, but that type is now an enum of orderable paths rather than an input object of fields taking a `SortDirection`.


### Why

An input object field must be given a value, so ordering by one property ascending, the common case, still cost a full object: `orderBy: {property: ascending}`. As an enum, each path is a single value, and a single value coerces to a one item list, so the common case is `orderBy: property`.


### The generated type

For an entity `Person` with a `Company` reference navigation:

Before:

```graphql
input PersonOrderBy {
  id: SortDirection
  name: SortDirection
  company: CompanyOrderBy
  companyId: SortDirection
}
```

After:

```graphql
enum PersonOrderBy {
  id
  id_desc
  name
  name_desc
  company_id
  company_id_desc
  company_name
  company_name_desc
  companyId
  companyId_desc
}
```

The field arguments are unchanged: `entities(orderBy: [PersonOrderBy!], ...)`. `SortDirection` is gone from the schema, as are the nested orderBy input types of entities that are only reachable through a navigation.


### Ascending

Before:

```graphql
{
  entities (orderBy: {property: ascending})
  {
    property
  }
}
```

After:

```graphql
{
  entities (orderBy: property)
  {
    property
  }
}
```


### Descending

Before:

```graphql
{
  entities (orderBy: {property: descending})
  {
    property
  }
}
```

After:

```graphql
{
  entities (orderBy: property_desc)
  {
    property
  }
}
```


### Multiple keys

Before:

```graphql
{
  entities (orderBy: [{property: descending}, {id: ascending}])
  {
    property
  }
}
```

After:

```graphql
{
  entities (orderBy: [property_desc, id])
  {
    property
  }
}
```


### Nested properties

A reference navigation is flattened into the path it leads to, joined with `_`.

Before:

```graphql
{
  entities (orderBy: {parent: {property: ascending}})
  {
    property
  }
}
```

After:

```graphql
{
  entities (orderBy: parent_property)
  {
    property
  }
}
```


### Variables

A variable is now a list of enum values rather than a list of objects.

Before:

```json
{"orderBy": [{"property": "descending"}]}
```

After:

```json
{"orderBy": ["property_desc"]}
```


### Nesting depth

An enum has to enumerate its values, so navigations are flattened to a fixed depth rather than the unbounded nesting the input object allowed. The default is 2, being the entity's own properties plus one reference navigation. Each extra level multiplies the number of enum values:

```cs
EfGraphQLConventions.RegisterInContainer<MyDbContext>(
    services,
    orderByEnumOptions: new()
    {
        NestingDepth = 3
    });
```

Flattening uses `_` as the separator, so a property whose name collides with a flattened navigation path throws at schema build, naming both paths.


### Keeping the input object style

The previous shape is still available, and keeps unbounded navigation nesting:

```cs
EfGraphQLConventions.RegisterInContainer<MyDbContext>(
    services,
    orderByStyle: OrderByStyle.Object);
```

The style applies to the whole container. Since both styles generate the type name `{Type}OrderBy`, registering two contexts with different styles fails at schema build when they share an entity.
