using System.Linq;
using GraphQL.Types;

public class Query : ObjectGraphType
{
    public Query()
    {
        Field<ListGraphType<CompanyGraph>>(
            "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies
                    .ToList();
            });
        Field<ListGraphType<EmployeeGraph>>(
            "employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees
                    .ToList();
            });
    }
}