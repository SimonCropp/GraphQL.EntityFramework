using GraphQL.EntityFramework;

public class EmployeeGraph :
    EfObjectGraphType<Employee>
{
    public EmployeeGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        Field(x => x.Age);
        AddNavigationField(
            name: "company",
            resolve: context => context.Source.Company);
    }
}