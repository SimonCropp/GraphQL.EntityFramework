using GraphQL.EntityFramework;

public class EmployeeSummaryGraphType :
    EfObjectGraphType<SampleDbContext, EmployeeSummary>
{
    public EmployeeSummaryGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}