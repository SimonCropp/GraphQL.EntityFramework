using System;

public class Child2Entity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithManyChildrenEntity? Parent { get; set; }
}