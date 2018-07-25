using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

[Table("IntegrationTestsParentEntity")]
public class ParentEntity
{
    public ParentEntity()
    {
        Children = new List<ChildEntity>();
    }
    public Guid Id { get; set; }
    public string Property { get; set; }
    public int? Nullable { get; set; }
    public IList<ChildEntity> Children { get; set; }
}