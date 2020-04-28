using GraphQL.EntityFramework;

public class CompanyGraph :
    EfObjectGraphType<SampleDbContext, Company>
{
    public CompanyGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
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