using System.ComponentModel.DataAnnotations;

public class ManyToManyLeftEntity
{
    [Key]
    public string Id { get; set; } = null!;

    public string? LeftName { get; set; }

    public IList<ManyToManyRightEntity> Rights { get; set; } = [];
}