using GraphQL.EntityFramework;
using GraphQL.Types;

public class MappingQuery :
    ObjectGraphType
{
    public MappingQuery(
        IEfGraphQLService<MappingContext> graphQlService)
    {
        graphQlService.AddSingleField(
            graph: this,
            name: "child",
            resolve: context => context.DbContext.Children);
        graphQlService.AddQueryField(
            graph: this,
            name: "children",
            resolve: context => context.DbContext.Children);
        graphQlService.AddSingleField(
            graph: this,
            name: "parent",
            resolve: context => context.DbContext.Parents);
        graphQlService.AddQueryField(
            graph: this,
            name: "parents",
            resolve: context => context.DbContext.Parents);
    }
}