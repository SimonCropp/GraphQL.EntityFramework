using GraphQL.EntityFramework;

public class CompanyGraphType :
    EfObjectGraphType<SampleDbContext, Company>
{
    public CompanyGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "employees",
            resolve: context => context.Source.Employees);
        AddNavigationConnectionField(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeNames: new[] {"Employees"});
        AutoMap();
    }
}