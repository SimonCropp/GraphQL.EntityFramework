using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

class IncludeAppender
{
    Dictionary<Type, List<Navigation>> navigations;

    public IncludeAppender(Dictionary<Type, List<Navigation>> navigations)
    {
        this.navigations = navigations;
    }

    public IQueryable<TItem> AddIncludes<TItem, TSource>(IQueryable<TItem> query, ResolveFieldContext<TSource> context)
        where TItem : class
    {
        var type = typeof(TItem);
        var navigationProperty = navigations[type];
        return AddIncludes(query, context.FieldDefinition, context.SubFields.Values, navigationProperty);
    }

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, FieldType fieldType, ICollection<Field> subFields, List<Navigation> navigationProperties)
        where T : class
    {
        var paths = GetPaths(fieldType, subFields, navigationProperties);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    List<string> GetPaths(FieldType fieldType, ICollection<Field> fields, List<Navigation> navigationProperty)
    {
        var list = new List<string>();

        var complexGraph = fieldType.GetComplexGraph();
        ProcessSubFields(list, null, fields, complexGraph, navigationProperty);
        return list;
    }

    void AddField(List<string> list, Field field, string? parentPath, FieldType fieldType, List<Navigation> parentNavigationProperties)
    {
        if (!fieldType.TryGetComplexGraph(out var complexGraph))
        {
            return;
        }

        var subFields = field.SelectionSet.Selections.OfType<Field>().ToList();
        if (IsConnectionNode(field))
        {
            if (subFields.Count > 0)
            {
                ProcessSubFields(list, parentPath, subFields, complexGraph!, parentNavigationProperties);
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

        ProcessSubFields(list, paths.First(), subFields, complexGraph!, navigations[entityType!]);
    }

    static IEnumerable<string> GetPaths(string? parentPath, string[] includeNames)
    {
        if (parentPath == null)
        {
            return includeNames;
        }

        return includeNames.Select(includeName => $"{parentPath}.{includeName}");
    }

    void ProcessSubFields(List<string> list, string? parentPath, ICollection<Field> subFields, IComplexGraphType complexGraph, List<Navigation> navigationProperties)
    {
        foreach (var subField in subFields)
        {
            var single = complexGraph.Fields.SingleOrDefault(x => x.Name == subField.Name);
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

    static bool TryGetIncludeMetadata(FieldType fieldType, out string[] value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (string[]) fieldNameObject;
            return true;
        }

        value = Array.Empty<string>();
        return false;
    }
}