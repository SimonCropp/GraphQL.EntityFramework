using System.Linq;
using GraphQL.Types;
using EfCoreGraphQL;

public class Query : ObjectGraphType
{
    public Query()
    {
        Field<ListGraphType<CompanyGraph>>(
            "companies",
            arguments: ArgumentAppender.DefaultArguments,
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies
                    .ApplyGraphQlArguments(context)
                    .ToList();
            });
        Field<ListGraphType<EmployeeGraph>>(
            "employees",
            arguments: ArgumentAppender.DefaultArguments,
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees
                    .ApplyGraphQlArguments(context)
                    .ToList();
            });
    }
}