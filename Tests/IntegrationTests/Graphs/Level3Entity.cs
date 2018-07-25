using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("IntegrationTestsLevel3Entity")]
public class Level3Entity
{
    public Guid Id { get; set; }
    public string Property { get; set; }
}