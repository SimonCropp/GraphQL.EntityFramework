public class FilterBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? CommonProperty { get; set; }

    // Many additional properties to simulate a "rich" entity
    public string? Field1 { get; set; }
    public string? Field2 { get; set; }
    public string? Field3 { get; set; }
    public int Field4 { get; set; }
    public int Field5 { get; set; }
    public DateTime? Field6 { get; set; }
    public DateTime? Field7 { get; set; }
    public bool Field8 { get; set; }
    public bool Field9 { get; set; }
    public Guid? Field10 { get; set; }
}
