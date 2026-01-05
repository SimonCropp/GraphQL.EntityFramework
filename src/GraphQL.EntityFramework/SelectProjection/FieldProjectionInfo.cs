record FieldProjectionInfo(
    List<string> ScalarFields,
    List<string> KeyNames,
    Dictionary<string, NavigationProjectionInfo> Navigations);