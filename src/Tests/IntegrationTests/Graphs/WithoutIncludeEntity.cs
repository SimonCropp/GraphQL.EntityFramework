using System;
using Xunit;

public class WithoutIncludeEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public WithoutIncludeChildEntity Child { get; set; } = null!;
} 