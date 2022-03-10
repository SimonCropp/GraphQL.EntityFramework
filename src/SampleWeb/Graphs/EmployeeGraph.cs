using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(Employee))]
public class EmployeeGraph :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}