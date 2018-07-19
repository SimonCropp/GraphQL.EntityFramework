using GraphQL.EntityFramework;

public class CompanyGraph : EfObjectGraphType<Company>
{
    public CompanyGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        AddListField< EmployeeGraph, Employee>(
            name: "employees",
            resolve: context => context.Source.Employees);
        AddListConnectionField<EmployeeGraph, Employee>(
            name: "employeesConnection",
            resolve: context => context.Source.Employees,
            includeName: "Employees");
    }
}