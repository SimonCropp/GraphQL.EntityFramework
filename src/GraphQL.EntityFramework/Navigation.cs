using System;
using System.Diagnostics;

namespace GraphQL.EntityFramework
{
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public class Navigation
    {
        public Navigation(string name, Type type, bool isNullable, bool isCollection)
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(type), type);
            Name = name;
            Type = type;
            IsNullable = isNullable;
            IsCollection = isCollection;
        }

        public string Name { get; }
        public Type Type { get; }
        public bool IsNullable { get; }
        public bool IsCollection { get; }
    }
}