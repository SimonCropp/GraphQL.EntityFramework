using GraphQL.EntityFramework;

public class EmployeeGraph :
    EfObjectGraphType<GraphQlEfSampleDbContext, Employee>
{
    public EmployeeGraph(IEfGraphQLService<GraphQlEfSampleDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Content, true);
        Field(x => x.Age);
        AddNavigationField(
            name: "company",
            resolve: context => context.Source.Company);
    }
}