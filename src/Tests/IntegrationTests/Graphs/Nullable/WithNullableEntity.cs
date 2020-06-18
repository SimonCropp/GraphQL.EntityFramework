using System;

public class WithNullableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? Nullable { get; set; }
}