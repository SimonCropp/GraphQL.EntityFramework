using GraphQL.EntityFramework;

public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddEnumerableField< EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
        AddEnumerableConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeName: "Employees");
    }
}