using GraphQL.Types;

public class CompanyGraph : ObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        Field<ListGraphType<EmployeeGraph>>(
            "employees",
            resolve: context =>
            {
                return context.Source.Employees;
            });
    }
}