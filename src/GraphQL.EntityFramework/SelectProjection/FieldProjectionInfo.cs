record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    IReadOnlySet<string> ForeignKeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations);
