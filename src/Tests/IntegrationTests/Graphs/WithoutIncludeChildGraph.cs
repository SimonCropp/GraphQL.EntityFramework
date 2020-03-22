using GraphQL.EntityFramework;

public class WithoutIncludeChildGraph :
    EfObjectGraphType<IntegrationDbContext, WithoutIncludeChildEntity>
{
    public WithoutIncludeChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(
            name: "parent",
            resolve: context =>
            {
                return context.Source.Parent;
            });
    }
}