using System.ComponentModel.DataAnnotations;

public class ManyToManyRightEntity
{
    public ManyToManyRightEntity() =>
        Lefts = new HashSet<ManyToManyLeftEntity>();

    [Key]
    public string Id { get; set; } = null!;

    public string? RightName { get; set; }

    public ICollection<ManyToManyLeftEntity> Lefts { get; set; }
}