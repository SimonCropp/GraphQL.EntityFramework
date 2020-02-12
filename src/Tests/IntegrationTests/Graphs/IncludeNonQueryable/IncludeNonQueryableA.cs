using System;
using Xunit;

public class IncludeNonQueryableA
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid IncludeNonQueryableBId { get; set; }
    public IncludeNonQueryableB IncludeNonQueryableB { get; set; } = null!;
}