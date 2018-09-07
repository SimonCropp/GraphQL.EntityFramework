using System;
using System.Collections.Generic;

public class FilterParentEntity
{
    public FilterParentEntity()
    {
        Children = new List<FilterChildEntity>();
    }
    public Guid Id { get; set; }
    public string Property { get; set; }
    public IList<FilterChildEntity> Children { get; set; }
}