using System.ComponentModel.DataAnnotations;

public class ManyToManyRightEntity
{
    [Key]
    public string Id { get; set; } = null!;

    public string? RightName { get; set; }

    public ICollection<ManyToManyLeftEntity> Lefts { get; set; } = new HashSet<ManyToManyLeftEntity>();
}