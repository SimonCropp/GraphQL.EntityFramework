using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

class IncludeAppender
{
    Dictionary<Type, Dictionary<string, Type>> navigations;

    public IncludeAppender(Dictionary<Type, Dictionary<string, Type>> navigations)
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

    IQueryable<T> AddIncludes<T>(IQueryable<T> query, FieldType fieldType, ICollection<Field> subFields, Dictionary<string, Type> navigationProperty)
        where T : class
    {
        foreach (var path in GetPaths(fieldType, subFields, navigationProperty))
        {
            query = query.Include(path);
        }

        return query;
    }

    IEnumerable<string> GetPaths(FieldType fieldType, ICollection<Field> fields, Dictionary<string, Type> navigationProperty)
    {
        var list = new List<string>();

        var complexGraph = fieldType.GetComplexGraph();
        ProcessSubFields(list, null, fields, complexGraph, navigationProperty);
        return list;
    }

    void AddField(List<string> list, Field field, string parentPath, FieldType fieldType, Dictionary<string, Type> parentNavigationProperties)
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

        if (parentNavigationProperties.TryGetValue(field.Name, out var propertyType))
        {
            var path = GetPath(parentPath, field, fieldType);
            list.Add(path);
            ProcessSubFields(list, path, subFields, complexGraph, navigations[propertyType]);
        }
    }

    void ProcessSubFields(List<string> list, string parentPath, ICollection<Field> subFields, IComplexGraphType complexGraph, Dictionary<string, Type> navigationProperties)
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

    public static Dictionary<string, object> GetIncludeMetadata(string value)
    {
        var metadata = new Dictionary<string, object>();
        if (value != null)
        {
            metadata["IncludeName"] = value;
        }

        return metadata;
    }

    public static void SetIncludeMetadata(FieldType fieldType, string value)
    {
        if (value != null)
        {
            fieldType.Metadata["IncludeName"] = value;
        }
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