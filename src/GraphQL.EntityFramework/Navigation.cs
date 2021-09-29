namespace GraphQL.EntityFramework;

[DebuggerDisplay("Name = {Name}, Type = {Type}")]
public record Navigation
(
    string Name,
    Type Type,
    bool IsNullable,
    bool IsCollection
);