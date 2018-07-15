using GraphQL.Types;
using GraphQL.EntityFramework;

public class Query : ObjectGraphType
{
    public Query()
    {
        this.AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });

        this.AddQueryConnectionField<CompanyGraph, Company>(
            name: "companiesConnection",
            includeName: "Companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Companies;
            });

        this.AddQueryField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            });


        this.AddQueryConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            includeName: "Employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Employees;
            });
    }
}