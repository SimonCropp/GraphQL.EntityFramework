using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class Company
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string? Content { get; set; }
    public List<Employee> Employees { get; set; } = null!;
}