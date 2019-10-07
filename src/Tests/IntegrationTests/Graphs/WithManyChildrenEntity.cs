using System;

public class WithManyChildrenEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public Child1Entity Child1 { get; set; } = null!;
    public Child2Entity Child2 { get; set; } = null!;
}