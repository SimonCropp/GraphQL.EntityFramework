using EfCoreGraphQL;
using GraphQL.Types;

public class CompanyGraph : ObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        this.AddEnumerableField<Company, EmployeeGraph, Employee>(
            "employees",
            resolve: context => context.Source.Employees);
        this.AddEnumerableConnectionField<Company, EmployeeGraph, Employee>(
            "employeesConnection",
            resolve: context => context.Source.Employees,
            "Employees");
    }
}