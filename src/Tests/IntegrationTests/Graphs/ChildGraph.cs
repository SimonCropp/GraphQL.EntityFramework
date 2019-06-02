using GraphQL.EntityFramework;

public class ChildGraph :
    EfObjectGraphType<IntegrationDbContext, ChildEntity>
{
    public ChildGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
        Field(x => x.Nullable, true);
        AddNavigationField(
            name: "parent",
            resolve: context => context.Source.Parent,
            graphType: typeof(ParentGraph));
        AddNavigationField(
            name: "parentAlias",
            resolve: context => context.Source.Parent,
            graphType: typeof(ParentGraph),
            includeNames: new[] {"Parent"});
    }
}