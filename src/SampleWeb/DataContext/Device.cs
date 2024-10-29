public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    [ForeignKey("DeviceId")]
    [InverseProperty("Devices")]
    public virtual ICollection<Employee> Employees { get; set; } = [];
}
