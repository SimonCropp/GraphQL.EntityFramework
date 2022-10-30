[Owned]
public class StreetAddress
{
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string State { get; set; } = null!;
    public string AreaCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}