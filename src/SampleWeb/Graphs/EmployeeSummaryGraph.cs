using GraphQL.EntityFramework;

public class EmployeeSummaryGraph :
    EfObjectGraphType<EmployeeSummary>
{
    public EmployeeSummaryGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.CompanyId);
        Field(x => x.AverageAge);
    }
}