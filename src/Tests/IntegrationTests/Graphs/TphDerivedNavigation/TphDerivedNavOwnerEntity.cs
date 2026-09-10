// Owns TPH base typed navigations, so a fragment on a derived type sits below a navigation
public class TphDerivedNavOwnerEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? ItemId { get; set; }
    public TphDerivedNavBaseEntity? Item { get; set; }
    public IList<TphDerivedNavBaseEntity> Items { get; set; } = [];
}
