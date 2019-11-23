using System;
using Xunit;

public class Child1Entity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithManyChildrenEntity? Parent { get; set; }
}