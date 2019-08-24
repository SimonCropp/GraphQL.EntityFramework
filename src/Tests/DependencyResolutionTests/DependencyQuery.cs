using GraphQL.EntityFramework;

public class DependencyQuery :
    QueryGraphType<DependencyDbContext>
{
    public DependencyQuery(IEfGraphQLService<DependencyDbContext> efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "entities",
            resolve: context => context.DbContext.Entities);
    }
}