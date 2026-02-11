public class TphAttachmentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid RequestId { get; set; }
    public TphRootEntity? Request { get; set; }
}
