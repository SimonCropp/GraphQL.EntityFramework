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
        AutoMap();
    }
}