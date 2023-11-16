public class WithManyChildrenGraphType :
    EfObjectGraphType<IntegrationDbContext, WithManyChildrenEntity>
{
    public WithManyChildrenGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "child1",
            resolve: context =>
            {
                Assert.NotNull(context.Source.Child2);
                Assert.NotNull(context.Source.Child1);
                return context.Source.Child1;
            },
            includeNames: [ "Child2", "Child1" ]);
        AutoMap();
    }
}