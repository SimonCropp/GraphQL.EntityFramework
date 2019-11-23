using System;
using Xunit;

public class WithNullableEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public int? Nullable { get; set; }
}