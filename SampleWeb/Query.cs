using GraphQL.Types;
using GraphQL.EntityFramework;

public class Query : ObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService)
    {
        efGraphQlService.AddQueryField<CompanyGraph, Company>(this, name: "companies", resolve: context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            return dataContext.Companies;
        });

        efGraphQlService.AddQueryConnectionField<CompanyGraph, Company>(this, name: "companiesConnection", resolve: context =>
        {
            var dataContext = (MyDataContext)context.UserContext;
            return dataContext.Companies;
        });

        efGraphQlService.AddQueryField<EmployeeGraph, Employee>(this, name: "employees", resolve: context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            return dataContext.Employees;
        });


        efGraphQlService.AddQueryConnectionField<EmployeeGraph, Employee>(this, name: "employeesConnection", resolve: context =>
        {
            var dataContext = (MyDataContext)context.UserContext;
            return dataContext.Employees;
        });
    }
}