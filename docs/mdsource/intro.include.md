Add [EntityFramework Core IQueryable](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.system-linq-iqueryable-provider) support to [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)

## Features

- **Automatic Query Generation** - Maps GraphQL queries to EF Core queries
- **Smart Projections** - Only loads requested fields from the database for optimal performance
- **Foreign Key Inclusion** - Automatically includes primary and foreign keys in projections for reliable custom resolvers
- **Filtering & Sorting** - Built-in support for complex `where`, `orderBy`, `skip`, and `take` arguments
- **Pagination** - GraphQL Relay connection specification support with `first`/`after` and `last`/`before`
- **Navigation Properties** - Automatic includes based on requested GraphQL fields
- **Multi-Context Support** - Use multiple DbContext types in the same GraphQL schema