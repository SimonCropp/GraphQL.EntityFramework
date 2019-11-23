using System;
using Xunit;

public class IncludeNonQueryableB
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid IncludeNonQueryableAId { get; set; }
    public IncludeNonQueryableA IncludeNonQueryableA { get; set; } = null!;
}