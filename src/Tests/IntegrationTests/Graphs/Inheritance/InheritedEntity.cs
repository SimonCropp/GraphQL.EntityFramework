using System;
using System.Collections.Generic;

public abstract class InheritedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<DerivedChildEntity> ChildrenFromBase { get; set; } = new List<DerivedChildEntity>();
}