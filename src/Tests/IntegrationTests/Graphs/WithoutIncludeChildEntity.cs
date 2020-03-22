using System;
using Xunit;

public class WithoutIncludeChildEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithoutIncludeEntity Parent { get; set; } = null!;
}
