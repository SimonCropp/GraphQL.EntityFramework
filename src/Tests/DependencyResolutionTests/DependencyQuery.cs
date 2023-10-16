public class DependencyQuery :
    QueryGraphType<DependencyDbContext>
{
    public DependencyQuery(IEfGraphQLService<DependencyDbContext> efGraphQlService) :
        base(efGraphQlService) =>
        AddQueryField(
            name: "entities",
            resolve: _ => _.DbContext.Entities);
}