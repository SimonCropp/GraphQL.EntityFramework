using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("IntegrationTestsChildEntity")]
public class ChildEntity
{
    public Guid Id { get; set; }
    public string Property { get; set; }
    public int? Nullable { get; set; }
    public Guid ParentId { get; set; }
    public ParentEntity Parent { get; set; }
}