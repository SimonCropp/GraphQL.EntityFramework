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
            resolve: context =>
            {
                return context
                    .Source
                    .Employees;
            });
    }
}