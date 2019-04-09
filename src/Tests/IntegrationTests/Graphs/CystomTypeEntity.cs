using System;

public class CustomTypeEntity
{
    public Guid Id { get; set; } = SimpleSequentialGuid.NewGuid();
    public long Property { get; set; }
}