using System;
using Xunit;

public class CustomTypeEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public long Property { get; set; }
}