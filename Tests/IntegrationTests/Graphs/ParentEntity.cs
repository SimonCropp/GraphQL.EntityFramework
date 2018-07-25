using System;
using System.Collections.Generic;

public class ParentEntity
{
    public ParentEntity()
    {
        Children = new List<ChildEntity>();
    }
    public Guid Id { get; set; }
    public string Property { get; set; }
    public IList<ChildEntity> Children { get; set; }
}