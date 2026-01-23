public class EmployeeEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public Guid DepartmentId { get; set; }
    public DepartmentEntity? Department { get; set; }
}
