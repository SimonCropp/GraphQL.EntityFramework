public class Employee
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public string Content { get; set; }
}