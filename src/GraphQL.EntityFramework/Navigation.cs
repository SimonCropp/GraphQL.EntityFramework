using System;
using System.Diagnostics;

namespace GraphQL.EntityFramework
{
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public record Navigation
    (
        //TODO: guard Name
        // Guard.AgainstNullWhiteSpace(nameof(name), name);
        string Name,
        Type Type,
        bool IsNullable,
        bool IsCollection
    );
}