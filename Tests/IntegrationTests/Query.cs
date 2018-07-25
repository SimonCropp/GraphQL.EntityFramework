using GraphQL.EntityFramework;
using GraphQL.Types;

public class Query : ObjectGraphType
{
    public Query(EfGraphQLService efGraphQlService)
    {
        efGraphQlService.AddQueryField<Level1Graph, Level1Entity>(this,
            name: "level1Entities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            });

        efGraphQlService.AddQueryField<ParentGraph, ParentEntity>(this,
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryField<ChildGraph, ChildEntity>(this,
            name: "childEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<ChildGraph, ChildEntity>(this,
            name: "childEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });
    }
}