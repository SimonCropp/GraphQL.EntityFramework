using System;

public class WithNullableEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public int? Nullable { get; set; }
}