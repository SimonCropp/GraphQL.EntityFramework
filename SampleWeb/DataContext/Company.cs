using System.Collections.Generic;

public class Company
{
    public int Id { get; set; }
    public string Content { get; set; }
    public List<Employee> Employees { get; set; } = new List<Employee>();
}