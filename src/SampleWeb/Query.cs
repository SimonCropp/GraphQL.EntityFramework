using GraphQL.Types;

#region QueryUsedInController

public class Query :
    QueryGraphType<SampleDbContext>
{
    public Query(IEfGraphQLService<SampleDbContext> efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "companies",
            resolve: _ => _.DbContext.Companies);

        #endregion

        AddSingleField(
            resolve: _ => _.DbContext.Companies,
            name: "company");

        AddSingleField(
            resolve: _ => _.DbContext.Companies,
            name: "companyById",
            idOnly: true);

        AddSingleField(
            resolve: _ => _.DbContext.Companies,
            name: "companyOrNull",
            nullable: true);

        AddQueryConnectionField(
            name: "companiesConnection",
            resolve: _ => _.DbContext.Companies.OrderBy(_=>_.Id));

        AddQueryField(
            name: "employees",
            resolve: _ => _.DbContext.Employees);

        AddQueryField(
            name: "employeesByArgument",
            resolve: context =>
            {
                var content = context.GetArgument<string>("content");
                return context.DbContext.Employees.Where(_ => _.Content == content);
            })
            .Argument<StringGraphType>("content");

        AddQueryConnectionField(
            name: "employeesConnection",
            resolve: _ => _.DbContext.Employees.OrderBy(_ => _.Content));

        #region ManuallyApplyWhere

        Field<ListGraphType<EmployeeSummaryGraphType>>("employeeSummary")
            .Argument<ListGraphType<WhereExpressionGraph>>("where")
            .Resolve(context =>
            {
                var dbContext = ResolveDbContext(context);
                IQueryable<Employee> query = dbContext.Employees;

                if (context.HasArgument("where"))
                {
                    var wheres = context.GetArgument<List<WhereExpression>>("where");

                    var predicate = ExpressionBuilder<Employee>.BuildPredicate(wheres);
                    query = query.Where(predicate);
                }

                return from q in query
                    group q by new
                    {
                        q.CompanyId
                    }
                    into g
                    select new EmployeeSummary
                    {
                        CompanyId = g.Key.CompanyId,
                        AverageAge = g.Average(_ => _.Age),
                    };
            });

        #endregion
    }
}