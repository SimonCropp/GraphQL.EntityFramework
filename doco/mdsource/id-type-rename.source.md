# Id type rename

In version 5, the `Id` type changed from `String` to `ID`. For queries that use named variables, this can cause type validations errors if the client and server are out of sync.

For example:

A version 4 query would look like this:

```graphql
query ($id: String!)
{
  MyEntities(id:$id)
  {
    id
  }
}
```
A version 5 query would look like this:

```graphql
query ($id: ID!)
{
  MyEntities(id:$id)
  {
    id
  }
}
```

Using the version 4 query against a version 5 server would result in:

```
Variable "$id" of type "String!" used in position expecting type "ID".
```

To work around this it is necessary to use a [ValidationRule](https://graphql-dotnet.github.io/docs/getting-started/query-validation) to change the variable type. There is an included ValidationRule included that achieves this:

snippet: FixIdTypeRule.cs

To use this rule set `ExecutionOptions.ValidationRules` to `FixIdTypeRule.CoreRulesWithIdFix`:

snippet: ExecutionOptionsWithFixIdTypeRule