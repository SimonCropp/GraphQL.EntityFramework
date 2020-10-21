using System;

public class CustomTypeEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Property { get; set; }
}