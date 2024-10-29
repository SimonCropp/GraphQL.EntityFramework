using System.ComponentModel.DataAnnotations;

public class ManyToManyShadowLeftEntity
{
    [Key]
    public string Id { get; set; } = null!;

    [ForeignKey("ManyToManyShadowLeftEntityId")]
    [InverseProperty("ManyToManyShadowLeftEntities")]
    public virtual ICollection<ManyToManyShadowRightEntity> ManyToManyShadowRightEntities { get; set; } = [];
}
