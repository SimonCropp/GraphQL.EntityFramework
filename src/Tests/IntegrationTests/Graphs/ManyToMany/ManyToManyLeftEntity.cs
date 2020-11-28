using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ManyToManyLeftEntity
{
    public ManyToManyLeftEntity()
    {
        Rights = new HashSet<ManyToManyRightEntity>();
    }

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? LeftName { get; set; }

    public IEnumerable<ManyToManyRightEntity> Rights { get; set; }
}