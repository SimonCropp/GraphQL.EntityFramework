using GraphQL.Types;
using GraphQL.EntityFramework;

public class Query : ObjectGraphType
{
    public Query(EfGraphQLService efGraphQlService)
    {
        this.AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            },
            efGraphQlService: efGraphQlService);

        this.AddQueryConnectionField<CompanyGraph, Company>(
            name: "companiesConnection",
            includeName: "Companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Companies;
            },
            efGraphQlService: efGraphQlService);

        this.AddQueryField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            },
            efGraphQlService: efGraphQlService);


        this.AddQueryConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            includeName: "Employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Employees;
            },
            efGraphQlService: efGraphQlService);
    }
}