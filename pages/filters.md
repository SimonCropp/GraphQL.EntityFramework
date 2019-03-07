<!--
This file was generate by MarkdownSnippets.
Source File: \pages\filters.source.md
To change this file edit the source file and then re-run the generation using either the dotnet global tool (https://github.com/SimonCropp/MarkdownSnippets#githubmarkdownsnippets) or using the api (https://github.com/SimonCropp/MarkdownSnippets#running-as-a-unit-test).
-->

# Filters

Sometimes, in the context of constructing an EF query, it is not possible to know if any given item should be returned in the results. For example when performing authorization where the rules rules are pulled from a different system, and that information does not exist in the database.

`GlobalFilters` allows a custom function to be executed after the EF query execution and determine if any given node should be included in the result.

Notes:

 * When evaluated on nodes of a collection, excluded nodes will be removed from collection.
 * When evaluated on a property node, the value will be replaced with null.
 * When doing paging or counts, there is currently no smarts that adjust counts or pages sizes when items are excluded. If this is required submit a PR that adds this feature, or don't mix filters with paging.
 * The filter is passed the current [User Context](https://graphql-dotnet.github.io/docs/getting-started/user-context) and the node item instance.
 * Filters will not be executed on null item instance.
 * A [Type.IsAssignableFrom](https://docs.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom) check will be performed to determine if an item instance should be filtered based on the `<TItem>`.
 * Filters are static and hence shared for the current [AppDomain](https://docs.microsoft.com/en-us/dotnet/api/system.appdomain).


### Signature:

<!-- snippet: GlobalFiltersSignature -->
```cs
public class GlobalFilters
{
    public delegate bool Filter<in T>(object userContext, T input);
    public delegate Task<bool> AsyncFilter<in T>(object userContext, T input);
```
<sup>[snippet source](/src/GraphQL.EntityFramework/Filter/GlobalFilters.cs#L8-L14)</sup>
<!-- endsnippet -->


### Usage:

<!-- snippet: add-filter -->
```cs
public class MyEntity
{
    public string Property { get; set; }
}
```
<sup>[snippet source](/src/Snippets/GlobalFilterSnippets.cs#L8-L15)</sup>
```cs
var filters = new GlobalFilters();
filters.Add<MyEntity>(
    (userContext, item) =>
    {
        return item.Property != "Ignore";
    });
EfGraphQLConventions.RegisterInContainer(services, model, filters);
```
<sup>[snippet source](/src/Snippets/GlobalFilterSnippets.cs#L19-L29)</sup>
<!-- endsnippet -->
