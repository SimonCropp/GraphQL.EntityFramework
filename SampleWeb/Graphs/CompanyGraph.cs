using GraphQL.EntityFramework;

public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddNavigationField< EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
        AddNavigationConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeName: "Employees");
    }
}