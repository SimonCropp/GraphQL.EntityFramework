public class ParentGraphType :
    EfObjectGraphType<IntegrationDbContext, ParentEntity>
{
    public ParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: _ => _.Source.Children,
            includeNames: [ "Children" ]);
        AddNavigationConnectionField(
            name: "childrenConnectionOmitQueryArguments",
            resolve: _ => _.Source.Children,
            includeNames: [ "Children" ],
            omitQueryArguments: true);

        #region ProjectedFieldBasicTransform

        AddProjectedNavigationField<string?, string>(
            name: "propertyUpper",
            projection: source => source.Property,
            transform: property => property?.ToUpper() ?? "");

        #endregion

        #region ProjectedFieldAsyncTransform

        AddProjectedNavigationField<string?, string>(
            name: "propertyUpperAsync",
            projection: source => source.Property,
            transform: async property =>
            {
                await Task.Yield();
                return property?.ToUpper() ?? "";
            });

        #endregion

        #region ProjectedFieldContextAwareTransform

        AddProjectedNavigationField<string?, string>(
            name: "propertyWithContext",
            projection: source => source.Property,
            transform: (context, property) =>
            {
                var prefix = context.Source.Id.ToString()[..8];
                return $"{prefix}: {property ?? "null"}";
            });

        #endregion

        #region ProjectedFieldListField

        AddProjectedNavigationListField<ChildEntity, string?, string>(
            name: "childrenProperties",
            navigation: source => source.Children,
            projection: child => child.Property,
            transform: property => property ?? "empty");

        #endregion

        // Projected field with complex projection
        AddProjectedNavigationField<string, string>(
            name: "anonymousProjection",
            projection: source => source.Id + "|" + (source.Property ?? "null"),
            transform: data => data);

        #region Projected Navigation Connection Field

        AddProjectedNavigationConnectionField<ChildEntity, string?, string>(
            name: "childrenConnectionProjected",
            navigation: source => source.Children,
            projection: child => child.Property,
            transform: prop => prop?.ToUpper() ?? "EMPTY");

        AddProjectedNavigationConnectionField<ChildEntity, string?, string>(
            name: "childrenConnectionProjectedAsync",
            navigation: source => source.Children,
            projection: child => child.Property,
            transform: async prop =>
            {
                await Task.Yield();
                return prop?.ToUpper() ?? "EMPTY";
            });

        #endregion

        AutoMap();
    }
}
