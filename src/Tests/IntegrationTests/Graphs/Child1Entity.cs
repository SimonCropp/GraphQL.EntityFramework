using System;

public class Child1Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentId { get; set; }
    public WithManyChildrenEntity? Parent { get; set; }
}