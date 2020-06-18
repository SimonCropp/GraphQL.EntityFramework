using System;
using System.Collections.Generic;

public class WithMisNamedQueryParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IList<WithMisNamedQueryChildEntity> Children { get; set; } = new List<WithMisNamedQueryChildEntity>();
}