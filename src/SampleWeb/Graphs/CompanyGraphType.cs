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
            resolve: _ => _.Source.Employees,
            includeNames: ["Employees"]);
        AutoMap();
    }
}