public abstract class TphRootEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<TphAttachmentEntity> Attachments { get; set; } = [];
}
