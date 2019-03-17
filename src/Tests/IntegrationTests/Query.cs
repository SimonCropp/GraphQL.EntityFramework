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

        AddQueryField<WithManyChildrenGraph, WithManyChildrenEntity>(
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
            typeof(WithNullableGraph),
            name: "withNullableEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithNullableEntities;
            });

        efGraphQlService.AddQueryField(this,
            typeof(WithMisNamedQueryParentGraph),
            name: "misNamed",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithMisNamedQueryParentEntities;
            });

        efGraphQlService.AddQueryField(this,
            typeof(ParentGraph),
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryField(this,
            typeof(ChildGraph),
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

        efGraphQlService.AddQueryField(this,
            typeof(FilterParentGraph),
            name: "parentEntitiesFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<FilterParentGraph, FilterParentEntity>(this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddSingleField(this,
            name: "parentEntity",
            typeof(ParentGraph),
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            }
        );
    }
}