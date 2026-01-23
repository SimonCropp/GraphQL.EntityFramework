public class DepartmentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public IList<EmployeeEntity> Employees { get; set; } = [];
}
