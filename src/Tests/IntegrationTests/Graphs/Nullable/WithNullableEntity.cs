using System;

public class WithNullableEntity
{
    public Guid Id { get; set; } = SimpleSequentialGuid.NewGuid();
    public int? Nullable { get; set; }
}