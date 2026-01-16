public class CompanyGraphType :
    EfObjectGraphType<SampleDbContext, Company>
{
    public CompanyGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "employees",
            resolve: _ => _.Source.Employees);
        AddNavigationConnectionField(
            name: "employeesConnection",
            projection: _ => _.Employees,
            resolve: ctx => ctx.Projection);
        AutoMap();
    }
}