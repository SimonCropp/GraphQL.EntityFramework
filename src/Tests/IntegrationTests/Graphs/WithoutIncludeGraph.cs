using GraphQL.EntityFramework;

public class WithoutIncludeGraph :
    EfObjectGraphType<IntegrationDbContext, WithoutIncludeEntity>
{
    public WithoutIncludeGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "child",
            resolve: context =>
            {
                return context.Source.Child;
            });
    }
}