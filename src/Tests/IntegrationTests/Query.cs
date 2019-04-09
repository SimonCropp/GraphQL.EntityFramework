using GraphQL.EntityFramework;

public class Query :
    EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "customType",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.CustomTypeEntities;
            });

        AddQueryField(
            name: "skipLevel",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Level1Entities;
            },
            graphType: typeof(SkipLevelGraph));

        AddQueryField(
            name: "manyChildren",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.WithManyChildrenEntities;
            });

        AddQueryField(
            name: "level1Entities",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.Level1Entities;
            });

        AddQueryField(
            name: "withNullableEntities",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.WithNullableEntities;
            });

        AddQueryField(
            name: "misNamed",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.WithMisNamedQueryParentEntities;
            });

        AddQueryField(
            name: "parentEntities",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.ParentEntities;
            });

        AddQueryField(
            name: "childEntities",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.ChildEntities;
            });

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(
            this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.ParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<ChildGraph, ChildEntity>(
            this,
            name: "childEntitiesConnection",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.ChildEntities;
            });

        AddQueryField(
            name: "parentEntitiesFiltered",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.FilterParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<FilterParentGraph, FilterParentEntity>(
            this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.FilterParentEntities;
            });

        AddSingleField(
            name: "parentEntity",
            resolve: context =>
            {
                var dbContext = (MyDbContext) context.UserContext;
                return dbContext.ParentEntities;
            });
    }
}