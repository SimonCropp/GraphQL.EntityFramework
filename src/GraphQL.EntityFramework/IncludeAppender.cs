using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Language.AST;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

class IncludeAppender
{
    IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> navigations;

    public IncludeAppender(IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> navigations)
    {
        this.navigations = navigations;
    }

    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class
    {
        if (context.SubFields == null)
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

        AddField(list, context.FieldAst, context.FieldAst.SelectionSet, null, context.FieldDefinition, navigationProperty, context);

        return list;
    }

    void AddField(List<string> list, Field field, SelectionSet selectionSet, string? parentPath, FieldType fieldType, IReadOnlyList<Navigation> parentNavigationProperties, IResolveFieldContext context, IComplexGraphType? graph = null)
    {
        if (graph == null && !fieldType.TryGetComplexGraph(out graph))
        {
            return;
        }

        var subFields = selectionSet.Selections.OfType<Field>().ToList();

        foreach (var inlineFragment in selectionSet.Selections.OfType<InlineFragment>())
        {
            if (inlineFragment.Type.GraphTypeFromType(context.Schema) is IComplexGraphType graphFragment)
            {
                AddField(list, field, inlineFragment.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graphFragment);
            }
        }

        foreach (var fragmentSpread in selectionSet.Selections.OfType<FragmentSpread>())
        {
            var fragmentDefinition = context.Fragments.FindDefinition(fragmentSpread.Name);
            if (fragmentDefinition == null)
            {
                continue;
            }

            AddField(list, field, fragmentDefinition.SelectionSet, parentPath, fieldType, parentNavigationProperties, context, graph);
        }

        if (IsConnectionNode(field) || field == context.FieldAst)
        {
            if (subFields.Count > 0)
            {
                ProcessSubFields(list, parentPath, subFields, graph!, parentNavigationProperties, context);
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

        //todo: do a single check to avoid allocations
        var paths = GetPaths(parentPath, includeNames).ToList();
        foreach (var path in paths)
        {
            list.Add(path);
        }

        ProcessSubFields(list, paths.First(), subFields, graph!, navigations[entityType!], context);
    }

    static IEnumerable<string> GetPaths(string? parentPath, string[] includeNames)
    {
        if (parentPath == null)
        {
            return includeNames;
        }

        return includeNames.Select(includeName => $"{parentPath}.{includeName}");
    }

    void ProcessSubFields(List<string> list, string? parentPath, ICollection<Field> subFields, IComplexGraphType graph, IReadOnlyList<Navigation> navigationProperties, IResolveFieldContext context)
    {
        foreach (var subField in subFields)
        {
            var single = graph.Fields.SingleOrDefault(x => x.Name == subField.Name);
            if (single != null)
            {
                AddField(list, subField, subField.SelectionSet, parentPath, single, navigationProperties, context);
            }
        }
    }

    static bool IsConnectionNode(Field field)
    {
        var name = field.Name.ToLowerInvariant();
        return name == "edges" || name == "items" || name == "node";
    }

    public static Dictionary<string, object> GetIncludeMetadata(string fieldName, IEnumerable<string>? includeNames)
    {
        var metadata = new Dictionary<string, object>();
        SetIncludeMetadata(fieldName, includeNames, metadata);
        return metadata;
    }

    public static void SetIncludeMetadata(FieldType fieldType, string fieldName, IEnumerable<string>? includeNames)
    {
        SetIncludeMetadata(fieldName, includeNames, fieldType.Metadata);
    }

    static void SetIncludeMetadata(string fieldName, IEnumerable<string>? includeNames, IDictionary<string, object> metadata)
    {
        if (includeNames == null)
        {
            metadata["_EF_IncludeName"] = FieldNameToArray(fieldName);
        }
        else
        {
            metadata["_EF_IncludeName"] = includeNames.ToArray();
        }
    }

    static string[] FieldNameToArray(string fieldName)
    {
        return new[] { char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1) };
    }

    static bool TryGetIncludeMetadata(FieldType fieldType, [NotNullWhen(true)] out string[]? value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[])fieldNameObject;
            return true;
        }

        value = null;
        return false;
    }
}