using GraphQL.EntityFramework;
using Xunit;

public class WithManyChildrenGraph :
    EfObjectGraphType<WithManyChildrenEntity>
{
    public WithManyChildrenGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField(name: "child1",
            resolve: context =>
            {
                Assert.NotNull(context.Source.Child2);
                Assert.NotNull(context.Source.Child1);
                return context.Source.Child1;
            }, graphType: typeof(Child1Graph), includeNames: new []{ "Child2", "Child1" });
    }
}