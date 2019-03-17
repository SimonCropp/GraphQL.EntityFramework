using GraphQL.EntityFramework;

public class CompanyGraph :
    EfObjectGraphType<Company>
{
    public CompanyGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddNavigationField<Employee>(
            typeof(EmployeeGraph),
            name: "employees",
            resolve: context => context.Source.Employees);
        AddNavigationConnectionField(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            typeof(EmployeeGraph),
            includeNames: new[] {"Employees"});
    }
}