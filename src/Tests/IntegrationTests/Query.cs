using GraphQL.EntityFramework;

public class Query :
    EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            typeof(CustomTypeGraph),
            name: "customType",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.CustomTypeEntities;
            });

        AddQueryField(
            typeof(SkipLevelGraph),
            name: "skipLevel",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            });

        AddQueryField(typeof(WithManyChildrenGraph),
            name: "manyChildren",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithManyChildrenEntities;
            });

        AddQueryField(
            typeof(Level1Graph),
            name: "level1Entities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            });

        efGraphQlService.AddQueryField(this,
            name: "withNullableEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithNullableEntities;
            }, graphType: typeof(WithNullableGraph));

        efGraphQlService.AddQueryField(this,
            name: "misNamed",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithMisNamedQueryParentEntities;
            }, graphType: typeof(WithMisNamedQueryParentGraph));

        efGraphQlService.AddQueryField(this,
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            }, graphType: typeof(ParentGraph));

        efGraphQlService.AddQueryField(this,
            name: "childEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            }, graphType: typeof(ChildGraph));

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            },
            graphType: typeof(ParentGraph));

        efGraphQlService.AddQueryConnectionField<ChildGraph, ChildEntity>(this,
            name: "childEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            },
            graphType: typeof(ChildGraph));

        efGraphQlService.AddQueryField(this,
            name: "parentEntitiesFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.FilterParentEntities;
            }, graphType: typeof(FilterParentGraph));

        efGraphQlService.AddQueryConnectionField<FilterParentGraph, FilterParentEntity>(this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.FilterParentEntities;
            },
            graphType: typeof(FilterParentGraph));

        efGraphQlService.AddSingleField(this,
            name: "parentEntity",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            }, graphType: typeof(ParentGraph));
    }
}