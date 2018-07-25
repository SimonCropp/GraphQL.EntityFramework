using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("IntegrationTestsLevel1Entity")]
public class Level1Entity
{
    public Guid Id { get; set; }
    public int? Level2EntityId { get; set; }
    public Level2Entity Level2Entity { get; set; }
}