public class CompanyGraphType :
    EfObjectGraphType<SampleDbContext, Company>
{
    public CompanyGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "employees",
            projection: _ => _.Employees,
            resolve: _ => _.Projection);
        AddNavigationConnectionField(
            name: "employeesConnection",
            projection: _ => _.Employees,
            resolve: _ => _.Projection);
        AutoMap();
    }
}
