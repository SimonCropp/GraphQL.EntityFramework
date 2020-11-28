using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ManyToManyRightEntity
{
    public ManyToManyRightEntity()
    {
        Lefts = new HashSet<ManyToManyLeftEntity>();
    }

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? RightName { get; set; }

    public ICollection<ManyToManyLeftEntity> Lefts { get; set; }
}