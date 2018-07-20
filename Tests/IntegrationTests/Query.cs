using GraphQL.EntityFramework;
using GraphQL.Types;

public class Query : ObjectGraphType
{
    public Query(EfGraphQLService efGraphQlService)
    {
        efGraphQlService.AddQueryField<ParentGraph, ParentEntity>(this,
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            },
            includeName: "Parents");
    }
}