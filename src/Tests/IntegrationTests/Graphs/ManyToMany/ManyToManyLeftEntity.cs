using System.ComponentModel.DataAnnotations;

public class ManyToManyLeftEntity
{
    public ManyToManyLeftEntity() =>
        Rights = new HashSet<ManyToManyRightEntity>();

    [Key]
    public string Id { get; set; } = null!;

    public string? LeftName { get; set; }

    public IEnumerable<ManyToManyRightEntity> Rights { get; set; }
}