using System;
using Xunit;

public class Entity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}