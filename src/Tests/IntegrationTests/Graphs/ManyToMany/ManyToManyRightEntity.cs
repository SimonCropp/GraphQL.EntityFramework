using System.ComponentModel.DataAnnotations;

public class ManyToManyRightEntity
{
    [Key]
    public string Id { get; set; } = null!;

    public string? RightName { get; set; }

    public IList<ManyToManyLeftEntity> Lefts { get; set; } = [];
}