public class WithManyChildrenGraphType :
    EfObjectGraphType<IntegrationDbContext, WithManyChildrenEntity>
{
    public WithManyChildrenGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "child1",
            projection: _ => new { _.Child1, _.Child2 },
            resolve: ctx =>
            {
                Assert.NotNull(ctx.Projection.Child2);
                Assert.NotNull(ctx.Projection.Child1);
                return ctx.Projection.Child1;
            });
        // Child1 is not the first path in the projection
        AddNavigationField(
            name: "child1Second",
            projection: _ => new { _.Child2, _.Child1 },
            resolve: _ => _.Projection.Child1);
        AutoMap();
    }
}