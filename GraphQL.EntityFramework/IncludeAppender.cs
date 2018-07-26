using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
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
        var paths = GetPaths(fieldType, subFields, navigationProperties).ToList();
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return query;
    }

    IEnumerable<string> GetPaths(FieldType fieldType, ICollection<Field> fields, List<Navigation> navigationProperty)
    {
        var list = new List<string>();

        var complexGraph = fieldType.GetComplexGraph();
        ProcessSubFields(list, null, fields, complexGraph, navigationProperty);
        return list;
    }

    void AddField(List<string> list, Field field, string parentPath, FieldType fieldType, List<Navigation> parentNavigationProperties)
    {
        if (!fieldType.TryGetComplexGraph(out var complexGraph))
        {
            return;
        }

        var subFields = field.SelectionSet.Selections.OfType<Field>().ToList();
        if (IsConnectionNode(field))
        {
            if (subFields.Any())
            {
                ProcessSubFields(list, parentPath, subFields, complexGraph, parentNavigationProperties);
            }

            return;
        }

       if (fieldType.TryGetEntityTypeForField(out var entityType))
        {
            if (GetSkipMetadata(fieldType))
            {
                return;
            }
            var path = GetPath(parentPath, field, fieldType);
            list.Add(path);
            ProcessSubFields(list, path, subFields, complexGraph, navigations[entityType]);
        }
    }

    void ProcessSubFields(List<string> list, string parentPath, ICollection<Field> subFields, IComplexGraphType complexGraph, List<Navigation> navigationProperties)
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

    static string GetPath(string parentPath, Field field, FieldType fieldType)
    {
        var fieldName = GetFieldName(field, fieldType);

        if (parentPath == null)
        {
            return fieldName;
        }

        return $"{parentPath}.{fieldName}";
    }

    static string GetFieldName(Field field, FieldType fieldType)
    {
        if (fieldType != null)
        {
            if (TryGetIncludeMetadata(fieldType, out var fieldName))
            {
                return fieldName;
            }
        }

        return char.ToUpperInvariant(field.Name[0]) + field.Name.Substring(1);
    }

    public static Dictionary<string, object> GetIncludeMetadata(string value, bool skip = false)
    {
        var metadata = new Dictionary<string, object>();
        if (value != null)
        {
            metadata["IncludeName"] = value;
        }

        if (skip)
        {
            metadata["Skip"] = true;
        }

        return metadata;
    }

    public static void SetSkipMetadata(FieldType fieldType)
    {
        fieldType.Metadata["Skip"] = true;
    }
    public static void SetIncludeMetadata(FieldType fieldType, string value)
    {
        if (value != null)
        {
            fieldType.Metadata["IncludeName"] = value;
        }
    }

    static bool GetSkipMetadata(FieldType fieldType)
    {
        if (fieldType.Metadata.TryGetValue("Skip", out var fieldNameObject))
        {
            return (bool) fieldNameObject;
        }

        return false;
    }

    static bool TryGetIncludeMetadata(FieldType fieldType, out string value)
    {
        //TODO: use a better name
        if (fieldType.Metadata.TryGetValue("IncludeName", out var fieldNameObject))
        {
            value = (string) fieldNameObject;
            return true;
        }

        value = null;
        return false;
    }
}