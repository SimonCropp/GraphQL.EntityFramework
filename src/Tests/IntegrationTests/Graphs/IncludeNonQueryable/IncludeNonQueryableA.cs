using System;

public class IncludeNonQueryableA
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public Guid IncludeNonQueryableBId { get; set; }
    public IncludeNonQueryableB IncludeNonQueryableB { get; set; } = null!;
}