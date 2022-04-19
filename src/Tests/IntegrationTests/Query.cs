using GraphQL.EntityFramework;

public class Query :
    QueryGraphType<IntegrationDbContext>
{
    public Query(IEfGraphQLService<IntegrationDbContext> efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "customType",
            resolve: context => context.DbContext.CustomTypeEntities);

        AddQueryField(
            name: "skipLevel",
            resolve: context => context.DbContext.Level1Entities,
            graphType: typeof(SkipLevelGraph));

        AddQueryField(
            name: "queryFieldWithInclude",
            resolve: context =>
            {
                var dataContext = context.DbContext;
                return dataContext.IncludeNonQueryableBs.AsQueryable()
                    .Select(p => p.IncludeNonQueryableA);
            });

        AddQueryField(
            name: "manyChildren",
            resolve: context => context.DbContext.WithManyChildrenEntities);

        AddQueryField(
            name: "level1Entities",
            resolve: context => context.DbContext.Level1Entities);

        AddQueryField(
            name: "withNullableEntities",
            resolve: context => context.DbContext.WithNullableEntities);

        AddQueryField(
            name: "namedEntities",
            resolve: context => context.DbContext.NamedEntities);

        AddQueryField(
            name: "misNamed",
            resolve: context => context.DbContext.WithMisNamedQueryParentEntities);

        AddQueryField(
            name: "parentEntities",
            resolve: context => context.DbContext.ParentEntities);

        AddQueryField(
            name: "childEntities",
            resolve: context => context.DbContext.ChildEntities);

        efGraphQlService.AddQueryConnectionField<ParentGraphType, ParentEntity>(
            this,
            name: "parentEntitiesConnection",
            resolve: context => context.DbContext.ParentEntities);

        efGraphQlService.AddQueryConnectionField<ChildGraphType, ChildEntity>(
            this,
            name: "childEntitiesConnection",
            resolve: context => context.DbContext.ChildEntities);

        AddQueryField(
            name: "parentEntitiesFiltered",
            resolve: context => context.DbContext.FilterParentEntities);

        efGraphQlService.AddQueryConnectionField<FilterParentGraphType, FilterParentEntity>(
            this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context => context.DbContext.FilterParentEntities);

        AddSingleField(
            name: "parentEntity",
            resolve: context => context.DbContext.ParentEntities);

        AddSingleField(
            name: "parentEntityNullable",
            resolve: context => context.DbContext.ParentEntities,
            nullable: true);

        efGraphQlService.AddQueryConnectionField(
            this,
            itemGraphType: typeof(InterfaceGraphType),
            name: "interfaceGraphConnection",
            resolve: context => context.DbContext.InheritedEntities);

        AddQueryField(
            name: "manyToManyLeftEntities",
            resolve: context => context.DbContext.ManyToManyLeftEntities);

        AddQueryField(
            name: "manyToManyRightEntities",
            resolve: context => context.DbContext.ManyToManyRightEntities);

        AddQueryField(
            name: "parentEntityViews",
            resolve: context => context.DbContext.ParentEntityView);

        efGraphQlService.AddQueryConnectionField<ParentEntityViewGraphType, ParentEntityView>(
            this,
            name: "parentEntitiesViewConnection",
            resolve: context => context.DbContext.ParentEntityView);

        AddSingleField(
            name: "parentEntityView",
            resolve: context => context.DbContext.ParentEntityView);

        AddSingleField(
            name: "parentEntityViewNullable",
            resolve: context => context.DbContext.ParentEntityView,
            nullable: true);

        AddSingleField(
            name: "ownedParent",
            resolve: context => context.DbContext.OwnedParents,
            nullable: true);
    }
}