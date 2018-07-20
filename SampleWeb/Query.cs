using GraphQL.Types;
using GraphQL.EntityFramework;

public class Query : ObjectGraphType
{
    public Query(EfGraphQLService efGraphQlService)
    {
        efGraphQlService.AddQueryField<CompanyGraph, Company>(this, name: "companies", resolve: context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            return dataContext.Companies;
        });

        efGraphQlService.AddQueryConnectionField<CompanyGraph, Company>(this, name: "companiesConnection", includeName: "Companies", resolve: context =>
        {
            var dataContext = (MyDataContext)context.UserContext;
            return dataContext.Companies;
        });

        efGraphQlService.AddQueryField<EmployeeGraph, Employee>(this, name: "employees", resolve: context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            return dataContext.Employees;
        });


        efGraphQlService.AddQueryConnectionField<EmployeeGraph, Employee>(this, name: "employeesConnection", includeName: "Employees", resolve: context =>
        {
            var dataContext = (MyDataContext)context.UserContext;
            return dataContext.Employees;
        });
    }
}