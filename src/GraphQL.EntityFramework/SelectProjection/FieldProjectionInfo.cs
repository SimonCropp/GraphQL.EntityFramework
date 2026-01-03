namespace GraphQL.EntityFramework;

record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations);

record NavigationProjectionInfo(
    Type EntityType,
    bool IsCollection,
    FieldProjectionInfo Projection);
