using System;
using Xunit;

public class Child2Entity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithManyChildrenEntity? Parent { get; set; }
}