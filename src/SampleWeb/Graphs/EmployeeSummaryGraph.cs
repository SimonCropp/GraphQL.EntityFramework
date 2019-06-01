using GraphQL.EntityFramework;

public class EmployeeSummaryGraph :
    EfObjectGraphType<GraphQlEfSampleDbContext, EmployeeSummary>
{
    public EmployeeSummaryGraph(IEfGraphQLService<GraphQlEfSampleDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.CompanyId);
        Field(x => x.AverageAge);
    }
}