public class Employee
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string? Content { get; set; }
    public int Age { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Employees")]
    public virtual ICollection<Device> Devices { get; set; } = [];
}