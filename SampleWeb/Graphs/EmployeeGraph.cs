using GraphQL.EntityFramework;

public class EmployeeGraph : EfObjectGraphType<Employee>
{
    public EmployeeGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content);
        Field(typeof(CompanyGraph), "company", null, null, x => x.Source.Company);
    }
}