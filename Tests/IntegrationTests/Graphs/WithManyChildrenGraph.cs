using GraphQL.EntityFramework;
using Xunit;

public class WithManyChildrenGraph : EfObjectGraphType<WithManyChildrenEntity>
{
    public WithManyChildrenGraph(EfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField<Child1Graph, Child1Entity>(
            name: "child1",
            resolve: context =>
            {
                Assert.NotNull(context.Source.Child2);
                Assert.NotNull(context.Source.Child1);
                return context.Source.Child1;
            });
    }
}