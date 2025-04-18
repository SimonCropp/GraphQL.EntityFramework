﻿public class Query :
    QueryGraphType<IntegrationDbContext>
{
    public Query(IEfGraphQLService<IntegrationDbContext> efGraphQlService)
        :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "customType",
            resolve: _ => _.DbContext.CustomTypeEntities);

        AddQueryField(
            name: "skipLevel",
            resolve: _ => _.DbContext.Level1Entities,
            graphType: typeof(SkipLevelGraph));

        AddQueryField(
            name: "queryFieldWithInclude",
            resolve: context =>
            {
                var dataContext = context.DbContext;
                return dataContext
                    .IncludeNonQueryableBs.AsQueryable()
                    .Select(_ => _.IncludeNonQueryableA);
            });

        AddQueryField(
            name: "manyChildren",
            resolve: _ => _.DbContext.WithManyChildrenEntities);

        AddQueryField(
            name: "level1Entities",
            resolve: _ => _.DbContext.Level1Entities);

        AddQueryField(
            name: "withNullableEntities",
            resolve: _ => _.DbContext.WithNullableEntities);

        AddQueryField(
            name: "namedEntities",
            resolve: _ => _.DbContext.NamedEntities);

        AddQueryField(
            name: "misNamed",
            resolve: _ => _.DbContext.WithMisNamedQueryParentEntities);

        AddQueryField(
            name: "parentEntities",
            resolve: _ => _.DbContext.ParentEntities);

        AddQueryField(
            name: "childEntities",
            resolve: _ => _.DbContext.ChildEntities);

        AddQueryField(
            name: "dateEntities",
            resolve: _ => _.DbContext.DateEntities);

        AddQueryField(
            name: "enumEntities",
            resolve: _ => _.DbContext.EnumEntities);

        AddQueryField(
            name: "stringEntities",
            resolve: _ => _.DbContext.StringEntities);

        AddQueryField(
            name: "timeEntities",
            resolve: _ => _.DbContext.TimeEntities);

        efGraphQlService
            .AddQueryConnectionField<ParentGraphType, ParentEntity>(
                this,
                name: "parentEntitiesConnection",
                resolve: _ => _.DbContext.ParentEntities.OrderBy(_ => _.Id))
            .PageSize(10);

        efGraphQlService.AddQueryConnectionField<ChildGraphType, ChildEntity>(
            this,
            name: "childEntitiesConnection",
            resolve: _ => _.DbContext.ChildEntities.OrderBy(_ => _.Parent));

        AddQueryField(
            name: "parentEntitiesFiltered",
            resolve: _ => _.DbContext.FilterParentEntities);

        efGraphQlService
            .AddQueryConnectionField<FilterParentGraphType, FilterParentEntity>(
                this,
                name: "parentEntitiesConnectionFiltered",
                resolve: _ => _.DbContext.FilterParentEntities.OrderBy(_ => _.Id))
            .PageSize(10);

        AddSingleField(
            name: "parentEntity",
            resolve: _ => _.DbContext.ParentEntities);
        AddFirstField(
            name: "parentEntityFirst",
            resolve: _ => _.DbContext.ParentEntities);

        AddSingleField(
            name: "parentEntityWithNoArgs",
            resolve: _ => _.DbContext.ParentEntities
                .Where(_ => _.Id == new Guid("00000000-0000-0000-0000-000000000001")),
            omitQueryArguments: true);

        AddFirstField(
            name: "parentEntityWithNoArgsFirst",
            resolve: _ => _
                .DbContext.ParentEntities
                .Where(_ => _.Id == new Guid("00000000-0000-0000-0000-000000000001")),
            omitQueryArguments: true);

        AddSingleField(
            name: "parentEntityIdOnly",
            resolve: _ => _.DbContext.ParentEntities,
            idOnly: true);

        AddFirstField(
            name: "parentEntityIdOnlyFirst",
            resolve: _ => _.DbContext.ParentEntities,
            idOnly: true);

        AddSingleField(
            name: "parentEntityNullable",
            resolve: _ => _.DbContext.ParentEntities,
            nullable: true);

        AddFirstField(
            name: "parentEntityNullableFirst",
            resolve: _ => _.DbContext.ParentEntities,
            nullable: true);

        efGraphQlService.AddQueryConnectionField<InheritedEntity>(
            this,
            itemGraphType: typeof(InterfaceGraphType),
            name: "interfaceGraphConnection",
            resolve: _ => _.DbContext.InheritedEntities.OrderBy(_ => _.Property));

        AddQueryField(
            name: "manyToManyLeftEntities",
            resolve: _ => _.DbContext.ManyToManyLeftEntities);

        AddQueryField(
            name: "manyToManyRightEntities",
            resolve: _ => _.DbContext.ManyToManyRightEntities);

        AddQueryField(
            name: "parentEntityViews",
            resolve: _ => _.DbContext.ParentEntityView);

        efGraphQlService.AddQueryConnectionField<ParentEntityViewGraphType, ParentEntityView>(
            this,
            name: "parentEntitiesViewConnection",
            resolve: _ => _.DbContext.ParentEntityView.OrderBy(_ => _.Property));

        AddSingleField(
            name: "parentEntityView",
            resolve: _ => _.DbContext.ParentEntityView);
        AddFirstField(
            name: "parentEntityViewFirst",
            resolve: _ => _.DbContext.ParentEntityView);

        AddSingleField(
            name: "parentEntityViewNullable",
            resolve: _ => _.DbContext.ParentEntityView,
            nullable: true);
        AddFirstField(
            name: "parentEntityViewNullableFirst",
            resolve: _ => _.DbContext.ParentEntityView,
            nullable: true);

        AddSingleField(
            name: "ownedParent",
            resolve: _ => _.DbContext.OwnedParents,
            nullable: true);
        AddFirstField(
            name: "ownedParentFirst",
            resolve: _ => _.DbContext.OwnedParents,
            nullable: true);
    }
}