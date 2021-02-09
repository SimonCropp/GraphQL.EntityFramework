using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(EmployeeSummary))]
public class EmployeeSummaryGraph :
    EfObjectGraphType<SampleDbContext, EmployeeSummary>
{
    public EmployeeSummaryGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}