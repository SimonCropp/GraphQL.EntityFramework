using GraphQL.EntityFramework;

public class CompanyGraph :
    EfObjectGraphType<Company>
{
    public CompanyGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddNavigationField<Employee>(name: "employees",
            resolve: context => context.Source.Employees, graphType: typeof(EmployeeGraph));
        AddNavigationConnectionField(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            typeof(EmployeeGraph),
            includeNames: new[] {"Employees"});
    }
}