using EfCoreGraphQL;
using GraphQL.Types;

public class CompanyGraph : ObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        Field<ListGraphType<EmployeeGraph>>(
            "employees",
            arguments: ArgumentAppender.DefaultArguments,
            resolve: context =>
            {
                var sourceEmployees = context
                    .Source
                    .Employees;
                return sourceEmployees.ApplyGraphQlArguments(context);
            });
    }
}