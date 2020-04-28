using GraphQL.EntityFramework;

public class EmployeeGraph :
    EfObjectGraphType<SampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "company",
            resolve: context => context.Source.Company);
        AutoMap();
    }
}