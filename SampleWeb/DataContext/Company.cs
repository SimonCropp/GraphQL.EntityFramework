using System;
using System.Collections.Generic;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public IList<Employee> Employees { get; set; }
}