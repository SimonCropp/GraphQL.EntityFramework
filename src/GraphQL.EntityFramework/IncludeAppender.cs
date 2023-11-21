class IncludeAppender(IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> navigations)
{
    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class
    {
        if (context.SubFields is null)
        {
            return query;
        }

        var type = typeof(TItem);
        if (!navigations.TryGetValue(type, out var navigationProperty))
        {
            return query;
        }

        return AddIncludes(query, context, navigationProperty);
    }

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, IResolveFieldContext context, IReadOnlyList<Navigation> navigationProperties)
        where T : class
    {
        var paths = GetPaths(context, navigationProperties);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    List<string> GetPaths(IResolveFieldContext context, IReadOnlyList<Navigation> navigationProperty)
    {
        var list = new List<string>();

        AddField(list, context.FieldAst, context.FieldAst.SelectionSet!, null, context.FieldDefinition, navigationProperty, context);

        return list;
    }

    void AddField(List<string> list, GraphQLField field, GraphQLSelectionSet selectionSet, string? parentPath, FieldType fieldType, IReadOnlyList<Navigation> parentNavigationProperties, IResolveFieldContext context, IComplexGraphType? graph = null)
    {
        if (graph == null && !fieldType.TryGetComplexGraph(out graph))
        {
            return;
        }

        var subFields = selectionSet.Selections.OfType<GraphQLField>().ToList();

        foreach (var inlineFragment in selectionSet.Selections.OfType<GraphQLInlineFragment>())
        {
            if (inlineFragment.TypeCondition!.Type.GraphTypeFromType(context.Schema) is IComplexGraphType graphFragment)
            {
                AddField(list, field, inlineFragment.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graphFragment);
            }
        }

        foreach (var fragmentSpread in selectionSet.Selections.OfType<GraphQLFragmentSpread>())
        {
            var name = fragmentSpread.FragmentName.Name;
            var fragmentDefinition = context.Document.Definitions
                .OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(x=>x.FragmentName.Name == name);
            if (fragmentDefinition is null)
            {
                continue;
            }

            AddField(list, field, fragmentDefinition.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graph);
        }

        if (IsConnectionNode(field) || field == context.FieldAst)
        {
            if (subFields.Count > 0)
            {
                ProcessSubFields(list, parentPath, subFields, graph, parentNavigationProperties, context);
            }

            return;
        }

        if (!fieldType.TryGetEntityTypeForField(out var entityType))
        {
            return;
        }

        if (!TryGetIncludeMetadata(fieldType, out var includeNames))
        {
            return;
        }

        var paths = GetPaths(parentPath, includeNames).ToList();
        foreach (var path in paths)
        {
            list.Add(path);
        }

        ProcessSubFields(list, paths.First(), subFields, graph, navigations[entityType], context);
    }

    static IEnumerable<string> GetPaths(string? parentPath, string[] includeNames)
    {
        if (parentPath is null)
        {
            return includeNames;
        }

        return includeNames.Select(includeName => $"{parentPath}.{includeName}");
    }

    void ProcessSubFields(List<string> list, string? parentPath, List<GraphQLField> subFields, IComplexGraphType graph, IReadOnlyList<Navigation> navigationProperties, IResolveFieldContext context)
    {
        foreach (var subField in subFields)
        {
            var single = graph.Fields.SingleOrDefault(_ => _.Name == subField.Name);
            if (single is not null)
            {
                AddField(list, subField, subField.SelectionSet!, parentPath, single, navigationProperties, context);
            }
        }
    }

    static bool IsConnectionNode(GraphQLField field)
    {
        var name = field.Name.StringValue.ToLowerInvariant();
        return name is "edges" or "items" or "node";
    }

    public static void SetIncludeMetadata(FieldType fieldType, string fieldName, IEnumerable<string>? includeNames) =>
        SetIncludeMetadata(fieldName, includeNames, fieldType.Metadata);

    static void SetIncludeMetadata(string fieldName, IEnumerable<string>? includeNames, IDictionary<string, object?> metadata)
    {
        if (includeNames is null)
        {
            metadata["_EF_IncludeName"] = FieldNameToArray(fieldName);
        }
        else
        {
            metadata["_EF_IncludeName"] = includeNames.ToArray();
        }
    }

    static string[] FieldNameToArray(string fieldName) =>
        [ char.ToUpperInvariant(fieldName[0]) + fieldName[1..] ];

    static bool TryGetIncludeMetadata(FieldType fieldType, [NotNullWhen(true)] out string[]? value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[])fieldNameObject!;
            return true;
        }

        value = null;
        return false;
    }
}