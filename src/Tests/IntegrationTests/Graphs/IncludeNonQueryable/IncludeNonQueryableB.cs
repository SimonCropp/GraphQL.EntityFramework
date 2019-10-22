using System;

public class IncludeNonQueryableB
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public Guid IncludeNonQueryableAId { get; set; }
    public IncludeNonQueryableA IncludeNonQueryableA { get; set; } = null!;
}