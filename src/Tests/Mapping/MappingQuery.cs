public class MappingQuery :
    ObjectGraphType
{
    public MappingQuery(
        IEfGraphQLService<MappingContext> graphQlService)
    {
        graphQlService.AddSingleField(
            graph: this,
            name: "child",
            resolve: _ => _.DbContext.Children);
        graphQlService.AddFirstField(
            graph: this,
            name: "childFirst",
            resolve: _ => _.DbContext.Children);
        graphQlService.AddQueryField(
            graph: this,
            name: "children",
            resolve: _ => _.DbContext.Children);
        graphQlService.AddQueryField(
            graph: this,
            name: "childrenOmitQueryArguments",
            resolve: _ => _.DbContext.Children,
            omitQueryArguments: true);
        graphQlService.AddSingleField(
            graph: this,
            name: "parent",
            resolve: _ => _.DbContext.Parents);
        graphQlService.AddFirstField(
            graph: this,
            name: "parentFirst",
            resolve: _ => _.DbContext.Parents);
        graphQlService.AddQueryField(
            graph: this,
            name: "parents",
            resolve: _ => _.DbContext.Parents);
    }
}