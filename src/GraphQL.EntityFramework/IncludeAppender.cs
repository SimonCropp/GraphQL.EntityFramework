using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    public IQueryable<TItem> AddIncludes<TItem, TSource>(IQueryable<TItem> query, ResolveFieldContext<TSource> context)
        where TItem : class
    {
        if (context.SubFields == null)
        {
            return query;
        }

        var type = typeof(TItem);
        var navigationProperty = navigations[type];
        return AddIncludes(query, context.FieldDefinition, context.SubFields.Values, navigationProperty);
    }

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, FieldType fieldType, ICollection<Field> subFields, IReadOnlyList<Navigation> navigationProperties)
        where T : class
    {
        var paths = GetPaths(fieldType, subFields, navigationProperties);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    List<string> GetPaths(FieldType fieldType, ICollection<Field> fields, IReadOnlyList<Navigation> navigationProperty)
    {
        var list = new List<string>();

        var graph = fieldType.GetComplexGraph();
        ProcessSubFields(list, null, fields, graph, navigationProperty);
        return list;
    }

    void AddField(List<string> list, Field field, string? parentPath, FieldType fieldType, IReadOnlyList<Navigation> parentNavigationProperties)
    {
        if (!fieldType.TryGetComplexGraph(out var graph))
        {
            return;
        }

        var subFields = field.SelectionSet.Selections.OfType<Field>().ToList();
        if (IsConnectionNode(field))
        {
            if (subFields.Count > 0)
            {
                ProcessSubFields(list, parentPath, subFields, graph!, parentNavigationProperties);
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

        ProcessSubFields(list, paths.First(), subFields, graph!, navigations[entityType!]);
    }

    static IEnumerable<string> GetPaths(string? parentPath, string[] includeNames)
    {
        if (parentPath == null)
        {
            return includeNames;
        }

        return includeNames.Select(includeName => $"{parentPath}.{includeName}");
    }


    void ProcessSubFields(List<string> list, string? parentPath, ICollection<Field> subFields, IComplexGraphType graph, IReadOnlyList<Navigation> navigationProperties)
    {
        foreach (var subField in subFields)
        {
            var single = graph.Fields.SingleOrDefault(x => x.Name == subField.Name);
            if (single != null)
            {
                AddField(list, subField, parentPath, single, navigationProperties);
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
        return new[] {char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1)};
    }

    static bool TryGetIncludeMetadata(FieldType fieldType, [NotNullWhen(true)] out string[]? value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[]) fieldNameObject;
            return true;
        }

        value = null;
        return false;
    }
}