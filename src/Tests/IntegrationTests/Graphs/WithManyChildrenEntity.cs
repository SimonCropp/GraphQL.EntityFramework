using System;
using Xunit;

public class WithManyChildrenEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Child1Entity Child1 { get; set; } = null!;
    public Child2Entity Child2 { get; set; } = null!;
}