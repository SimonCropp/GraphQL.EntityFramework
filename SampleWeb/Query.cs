using GraphQL.EntityFramework;

public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) : base(efGraphQlService)
    {
        AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });

        AddQueryConnectionField<CompanyGraph, Company>(
            name: "companiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });

        AddQueryField<EmployeeGraph, Employee>(
            name: "employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            });

        AddQueryConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            });
    }
}