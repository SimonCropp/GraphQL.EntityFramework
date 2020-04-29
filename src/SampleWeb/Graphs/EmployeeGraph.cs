using GraphQL.EntityFramework;

public class EmployeeGraph :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}