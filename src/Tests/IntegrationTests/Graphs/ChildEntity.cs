using System;
using Xunit;

public class ChildEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
    public int? Nullable { get; set; }
    public Guid? ParentId { get; set; }
    public ParentEntity? Parent { get; set; }
}