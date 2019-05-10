using System;

public class CustomTypeEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public long Property { get; set; }
}