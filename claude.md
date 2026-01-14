# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GraphQL.EntityFramework is a .NET library that adds EntityFramework Core IQueryable support to GraphQL.NET. It enables automatic query generation, filtering, pagination, and ordering for GraphQL queries backed by EF Core.

## Build and Test Commands

### Building
```bash
dotnet build src --configuration Release
```

### Running Tests
```bash
# Run all tests except integration tests
dotnet test src --configuration Release --no-build --no-restore --filter Category!=Integration

# Run all tests including integration tests
dotnet test src --configuration Release --no-build --no-restore
```

### Running a Single Test
```bash
# Run a specific test by fully qualified name
dotnet test src --filter "FullyQualifiedName~TestNamespace.TestClass.TestMethod"

# Run all tests in a class
dotnet test src --filter "FullyQualifiedName~TestNamespace.TestClass"
```

### Documentation Generation
The README.md and docs/*.md files are auto-generated from source files using [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets). To regenerate documentation:
- Edit the corresponding `*.source.md` files in `/docs/mdsource/` or `/readme.source.md`
- Run MarkdownSnippets to regenerate the markdown files
- Never edit `*.md` files directly if they have a "GENERATED FILE - DO NOT EDIT" header

## Architecture

### Core Components

**EfGraphQLService** (`src/GraphQL.EntityFramework/GraphApi/EfGraphQLService*.cs`)
- The central service that provides methods to add GraphQL fields backed by EF queries
- Split across multiple partial class files by functionality:
  - `EfGraphQLService_QueryableConnection.cs` - Connection (pageable) fields for IQueryable
  - `EfGraphQLService_Navigation.cs` - Single navigation property fields
  - `EfGraphQLService_NavigationList.cs` - List navigation property fields
  - `EfGraphQLService_Single.cs` - Single entity queries (uses SingleOrDefaultAsync)
  - `EfGraphQLService_First.cs` - First entity queries (uses FirstOrDefaultAsync)
  - `EfGraphQLService_Queryable.cs` - General queryable fields

**EfObjectGraphType** (`src/GraphQL.EntityFramework/GraphApi/EfObjectGraphType.cs`)
- Base class for entity graph types that provides convenient wrapper methods around EfGraphQLService
- Supports AutoMap() to automatically map entity properties to GraphQL fields

**QueryGraphType** (base class for root queries)
- Provides entry points for GraphQL queries
- Derived from EfObjectGraphType but serves as the schema root

**ArgumentProcessor** (`src/GraphQL.EntityFramework/GraphApi/ArgumentProcessor.cs`)
- Parses GraphQL query arguments (where, orderBy, skip, take, ids) and applies them to IQueryable
- Converts GraphQL filter expressions into EF LINQ queries

**ExpressionBuilder** (`src/GraphQL.EntityFramework/Filters/`)
- Builds LINQ expressions from GraphQL where clause arguments
- Supports complex filtering including grouping, negation, and nested properties

**Filters** (`src/GraphQL.EntityFramework/Filters/Filters.cs`)
- Post-query filtering mechanism for authorization or business rules
- Executed after EF query to determine if nodes should be included in results
- Useful when filter criteria don't exist in the database

**ProjectedField API** (`src/GraphQL.EntityFramework/GraphApi/EfGraphQLService_Projected.cs`)
- Provides methods for fields that project and transform entity properties
- Three main methods:
  - `AddProjectedField` - For query-level projected fields
  - `AddProjectedNavigationField` - For single navigation property projections
  - `AddProjectedNavigationListField` - For collection navigation property projections
- Each accepts:
  - `projection` - Expression that selects required properties from entity
  - `transform` - Function that transforms projected data to final result
  - Supports both sync and async transforms
  - Supports context-aware transforms that receive ResolveEfFieldContext
- Use `includeNames` parameter to specify which properties/navigations to load
- Note: Current implementation has limitations with scalar property projection on source entities

**Roslyn Analyzer** (`src/GraphQL.EntityFramework.Analyzers/`)
- Compile-time code analyzer packaged with the library
- Diagnostic ID: GQLEF001
- Detects problematic `context.Source.PropertyName` access patterns
- Only analyzes code in EfObjectGraphType, EfInterfaceGraphType, and QueryGraphType classes
- Allows `Id` and `*Id` properties (primary keys and foreign keys)
- Warns on other property access, suggesting use of ProjectedField API
- Packaged in NuGet at `analyzers/dotnet/cs` directory

### Include Resolution

The library automatically determines EF includes by interrogating the incoming GraphQL query. When a navigation property is requested in a GraphQL query, the corresponding EF Include is automatically added to the query. This is handled by:
- Examining the GraphQL AST (Abstract Syntax Tree)
- Mapping field names to EF navigation properties
- Building the Include chain (e.g., "Friends.Address")

Field names are uppercased and used as include names by default, but can be overridden with the `includeNames` parameter.

### Container Registration

The library registers services via `EfGraphQLConventions.RegisterInContainer<TDbContext>()` which:
- Requires an EF `IModel` instance (either passed directly or resolved from container)
- Optionally accepts custom DbContext resolver delegate
- Optionally accepts custom Filters resolver delegate
- Supports `disableTracking` flag for AsNoTracking queries

### Multi-Context Support

Multiple DbContext types can be registered and used simultaneously:
- Register each with `EfGraphQLConventions.RegisterInContainer<TDbContext1>()` and `EfGraphQLConventions.RegisterInContainer<TDbContext2>()`
- Inject `IEfGraphQLService<TDbContext1>` and `IEfGraphQLService<TDbContext2>` separately
- Each graph type specifies which DbContext it uses via generic type parameter

### EfDocumentExecuter

Custom DocumentExecuter that uses SerialExecutionStrategy for queries instead of parallel execution. This prevents the "second operation started on context" error that occurs when multiple async fields resolve in parallel using the same DbContext instance.

### Connection Types (Pagination)

The library implements the GraphQL Relay connection specification:
- `AddQueryConnectionField` - For pageable root queries
- `AddNavigationConnectionField` - For pageable navigation properties
- Supports first/after and last/before pagination
- Includes totalCount, edges, cursor, and pageInfo

## Key Patterns

### Defining Graph Types

Use `EfObjectGraphType<TDbContext, TSource>` as base class and call:
- `AddNavigationField` - Single navigation property
- `AddNavigationListField` - Collection navigation property
- `AddNavigationConnectionField` - Pageable collection
- `AddQueryField` - IQueryable that returns multiple entities
- `AddSingleField` - IQueryable that returns single entity (throws if multiple)
- `AddFirstField` - IQueryable that returns first entity or null
- `AutoMap()` - Automatically map all properties

### Query Arguments

All query fields support standardized arguments:
- `ids` - Filter by ID(s)
- `where` - Complex filtering with comparisons (equal, contains, startsWith, etc.)
- `orderBy` - Sorting with path and descending flag
- `skip` - Skip N results
- `take` - Take N results

Arguments are processed in order: ids → where → orderBy → skip → take

### Projection Support

The library supports EF projections where you can use `Select()` to project to DTOs or anonymous types before applying GraphQL field resolution.

### ProjectedField Usage

The ProjectedField API provides a way to explicitly project and transform entity properties:

**Simple Transform:**
```csharp
AddProjectedNavigationField<ParentEntity, string?, string>(
    name: "propertyUpper",
    resolve: _ => _.Source,
    projection: entity => entity.Property,
    transform: property => property?.ToUpper() ?? "",
    includeNames: ["Property"]);
```

**Context-Aware Transform:**
```csharp
AddProjectedNavigationField<ParentEntity, string?, string>(
    name: "propertyWithContext",
    resolve: _ => _.Source,
    projection: entity => entity.Property,
    transform: (context, property) => {
        var userId = context.User?.FindFirst("sub")?.Value;
        return $"{userId}: {property ?? "null"}";
    },
    includeNames: ["Property"]);
```

**Async Transform:**
```csharp
AddProjectedNavigationField<ParentEntity, string?, string>(
    name: "enrichedProperty",
    resolve: _ => _.Source,
    projection: entity => entity.Property,
    transform: async property => {
        var result = await _externalService.EnrichAsync(property);
        return result;
    },
    includeNames: ["Property"]);
```

**List Field:**
```csharp
AddProjectedNavigationListField<ChildEntity, string?, string>(
    name: "childrenProperties",
    resolve: _ => _.Source.Children,
    projection: child => child.Property,
    transform: property => property ?? "empty",
    includeNames: ["Children", "Children.Property"]);
```

**Note:** The `includeNames` parameter is critical for ensuring EF loads the required properties/navigations.

## Testing

Tests use:
- xUnit v3
- Verify.XunitV3 for snapshot testing
- EfLocalDb for in-memory SQL Server testing
- SQL Server LocalDB for integration tests

The test project (`src/Tests/`) includes:
- `IntegrationTests/` - Full integration tests with real database
- `MultiContextTests/` - Tests for multiple DbContext scenarios
- Expression and filter unit tests
- Connection/pagination tests

## Code Style

- Uses C# 14+ features (global usings, file-scoped namespaces, record types)
- Implicit usings enabled via Directory.Build.props
- Treats warnings as errors
- Uses .editorconfig for code style enforcement
- Uses Fody/ConfigureAwait.Fody for ConfigureAwait(false) injection
