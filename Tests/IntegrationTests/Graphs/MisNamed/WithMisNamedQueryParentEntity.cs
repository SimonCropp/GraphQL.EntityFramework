using System;
using System.Collections.Generic;

public class WithMisNamedQueryParentEntity
{
    public WithMisNamedQueryParentEntity()
    {
        Children = new List<WithMisNamedQueryChildEntity>();
    }
    public Guid Id { get; set; }
    public IList<WithMisNamedQueryChildEntity> Children { get; set; }
}