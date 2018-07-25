using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("IntegrationTestsLevel2Entity")]
public class Level2Entity
{
    public Guid Id { get; set; }
    public Guid? Level3EntityId { get; set; }
    public Level3Entity Level3Entity { get; set; }
}