using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using GraphQL.Types;

#region QueryUsedInController

public class Query :
    EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField<CompanyGraph, Company>(
            name: "companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });

        #endregion
        AddSingleField<CompanyGraph, Company>(
            name: "company",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
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

        AddQueryField<EmployeeGraph, Employee>(
            name: "employeesByArgument",
            arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "content" }),
            resolve: context =>
            {
                var content = context.GetArgument<string>("content");
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.Employees.Where(x => x.Content == content);
            });

        AddQueryConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            });
        
        Field<ListGraphType<EmployeeSummaryGraph>>(
            name: "employeeSummary",
            arguments: new QueryArguments(
                new QueryArgument<ListGraphType<WhereExpressionGraph>> { Name = "where" }
            ),
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                IQueryable<Employee> query = dataContext.Employees;

                if (context.HasArgument("where"))
                {
                    var whereExpressions = context.GetArgument<List<WhereExpression>>("where");
                    foreach (var whereExpression in whereExpressions)
                    {
                        var predicate = ExpressionBuilder<Employee>.BuildPredicate(whereExpression);
                        query = query.Where(predicate);
                    }
                }

                var results = from q in query
                              group q by new { q.CompanyId } into g
                              select new EmployeeSummary {
                                CompanyId = g.Key.CompanyId,
                                AverageAge = g.Average(x => x.Age),
                              };

                return results;
            });
    }
}