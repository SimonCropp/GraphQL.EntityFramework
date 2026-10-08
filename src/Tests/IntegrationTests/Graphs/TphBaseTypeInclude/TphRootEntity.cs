public abstract class TphRootEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<TphAttachmentEntity> Attachments { get; set; } = [];

    // Read only, so a select cannot bind it. Selecting it puts a query on the includes path.
    public string Summary => $"Summary of {Property}";
}
