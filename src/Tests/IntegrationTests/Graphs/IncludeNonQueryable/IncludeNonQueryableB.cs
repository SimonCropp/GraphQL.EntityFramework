using System;

public class IncludeNonQueryableB
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IncludeNonQueryableAId { get; set; }
    public IncludeNonQueryableA IncludeNonQueryableA { get; set; } = null!;
}