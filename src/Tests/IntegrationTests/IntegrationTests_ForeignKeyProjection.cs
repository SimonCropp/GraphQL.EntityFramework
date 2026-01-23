// Test for foreign key projection bug fix
// https://github.com/SimonCropp/GraphQL.EntityFramework/issues/XXX
public partial class IntegrationTests
{
    [Fact]
    public async Task ForeignKey_CustomField_UsesProjectedForeignKey()
    {
        var query =
            """
            {
              departments (orderBy: {path: "name"})
              {
                id
                name
                isActive
                employees (orderBy: {path: "name"})
                {
                  id
                  name
                  departmentId
                  isInActiveDepartment
                }
              }
            }
            """;

        var activeDepartment = new DepartmentEntity
        {
            Name = "Active Department",
            IsActive = true
        };
        var inactiveDepartment = new DepartmentEntity
        {
            Name = "Inactive Department",
            IsActive = false
        };

        var employee1 = new EmployeeEntity
        {
            Name = "Alice",
            Department = activeDepartment,
            DepartmentId = activeDepartment.Id
        };
        var employee2 = new EmployeeEntity
        {
            Name = "Bob",
            Department = activeDepartment,
            DepartmentId = activeDepartment.Id
        };
        var employee3 = new EmployeeEntity
        {
            Name = "Charlie",
            Department = inactiveDepartment,
            DepartmentId = inactiveDepartment.Id
        };

        activeDepartment.Employees.Add(employee1);
        activeDepartment.Employees.Add(employee2);
        inactiveDepartment.Employees.Add(employee3);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [activeDepartment, inactiveDepartment, employee1, employee2, employee3]);
    }

    [Fact]
    public async Task ForeignKey_NestedNavigation_IncludesForeignKeyInProjection()
    {
        // This test verifies that foreign keys are included in nested navigation projections
        // Without the fix, DepartmentId would be Guid.Empty and the custom field would fail
        var query =
            """
            {
              departments (where: [{path: "name", comparison: equal, value: "Engineering"}])
              {
                name
                employees (orderBy: {path: "name"})
                {
                  name
                  departmentId
                  isInActiveDepartment
                }
              }
            }
            """;

        var department = new DepartmentEntity
        {
            Name = "Engineering",
            IsActive = true
        };

        var employee = new EmployeeEntity
        {
            Name = "Developer",
            Department = department,
            DepartmentId = department.Id
        };

        department.Employees.Add(employee);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [department, employee]);
    }
}
