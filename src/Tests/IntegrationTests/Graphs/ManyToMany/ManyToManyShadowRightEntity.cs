using System.ComponentModel.DataAnnotations;

public class ManyToManyShadowRightEntity
{
    [Key]
    public string Id { get; set; } = null!;

    [ForeignKey("ManyToManyShadowRightEntityId")]
    [InverseProperty("ManyToManyShadowRightEntities")]
    public virtual ICollection<ManyToManyShadowLeftEntity> ManyToManyShadowLeftEntities { get; set; } = [];
}
