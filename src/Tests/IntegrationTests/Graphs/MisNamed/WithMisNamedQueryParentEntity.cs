using System;
using System.Collections.Generic;
using Xunit;

public class WithMisNamedQueryParentEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public IList<WithMisNamedQueryChildEntity> Children { get; set; } = new List<WithMisNamedQueryChildEntity>();
}