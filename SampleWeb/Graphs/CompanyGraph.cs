using EfCoreGraphQL;

public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddEnumerableField< EmployeeGraph, Employee>(
            "employees",
            resolve: context => context.Source.Employees);
        AddEnumerableConnectionField<EmployeeGraph, Employee>(
            "employeesConnection",
            resolve: context => context.Source.Employees,
            "Employees");
    }
}