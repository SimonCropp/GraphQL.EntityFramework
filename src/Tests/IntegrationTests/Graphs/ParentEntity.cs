using System;
using System.Collections.Generic;
using Xunit;

public class ParentEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
    public IList<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}