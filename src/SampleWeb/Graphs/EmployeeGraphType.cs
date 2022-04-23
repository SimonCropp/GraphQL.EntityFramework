using GraphQL.EntityFramework;

public class EmployeeGraphType :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}