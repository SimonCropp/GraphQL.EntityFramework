using GraphQL.EntityFramework;

public class EmployeeSummaryGraph :
    EfObjectGraphType<SampleDbContext, EmployeeSummary>
{
    public EmployeeSummaryGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}