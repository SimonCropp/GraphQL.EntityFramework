using GraphQL.EntityFramework;

public class EmployeeSummaryGraph :
    EfObjectGraphType<SampleDbContext, EmployeeSummary>
{
    public EmployeeSummaryGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.CompanyId);
        Field(x => x.AverageAge);
    }
}