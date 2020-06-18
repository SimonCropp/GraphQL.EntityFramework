using System;
using System.Collections.Generic;

public class ParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}