using System;

public class IncludeNonQueryableA
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IncludeNonQueryableBId { get; set; }
    public IncludeNonQueryableB IncludeNonQueryableB { get; set; } = null!;
}