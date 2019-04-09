using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using GraphQL.Types;

#region QueryUsedInController

public class Query :
    QueryGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "companies",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Companies;
            });

        #endregion

        AddSingleField(
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Companies;
            },
            name: "company");

        AddQueryConnectionField(
            name: "companiesConnection",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Companies;
            });

        AddQueryField(
            name: "employees",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Employees;
            });

        AddQueryField(
            name: "employeesByArgument",
            resolve: context =>
            {
                var content = context.GetArgument<string>("content");
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Employees.Where(x => x.Content == content);
            },
            arguments: new QueryArguments(
                new QueryArgument<StringGraphType>
                {
                    Name = "content"
                }));

        AddQueryConnectionField(
            name: "employeesConnection",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Employees;
            });

        #region ManuallyApplyWhere

        Field<ListGraphType<EmployeeSummaryGraph>>(
            name: "employeeSummary",
            arguments: new QueryArguments(
                new QueryArgument<ListGraphType<WhereExpressionGraph>>
                {
                    Name = "where"
                }
            ),
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                IQueryable<Employee> query = dbContext.Employees;

                if (context.HasArgument("where"))
                {
                    var wheres = context.GetArgument<List<WhereExpression>>("where");
                    foreach (var where in wheres)
                    {
                        var predicate = ExpressionBuilder<Employee>.BuildPredicate(where);
                        query = query.Where(predicate);
                    }
                }

                return from q in query
                    group q by new {q.CompanyId}
                    into g
                    select new EmployeeSummary
                    {
                        CompanyId = g.Key.CompanyId,
                        AverageAge = g.Average(x => x.Age),
                    };
            });

        #endregion
    }
}