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
            ProcessSubFields(list, paths.First(), subFields, complexGraph, navigations[entityType]);
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

    static IEnumerable<string> GetPaths(string parentPath, List<string> includeNames)
    {
        foreach (var includeName in includeNames)
        {
            if (parentPath == null)
            {
                yield return includeName;
            }

            yield return $"{parentPath}.{includeName}";
        }
    }

    public static Dictionary<string, object> GetIncludeMetadata(IEnumerable<string> value)
    {
        var metadata = new Dictionary<string, object>();
        if (value != null)
        {
            metadata["_EF_IncludeName"] = value.ToList();
        }

        return metadata;
    }

    public static void SetIncludeMetadata(FieldType fieldType, string fieldName, IEnumerable<string> includeNames)
    {
        var metadata = fieldType.Metadata;
        if (includeNames == null)
        {
            metadata["_EF_IncludeName"] = new[] {char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1)};
        }
        else
        {
            metadata["_EF_IncludeName"] = includeNames.ToList();
        }
    }


    static bool TryGetIncludeMetadata(FieldType fieldType, out List<string> value)
    {
        if (fieldType.Metadata.TryGetValue("_EF_IncludeName", out var fieldNameObject))
        {
            value = (List<string>) fieldNameObject;
            return true;
        }

        value = null;
        return false;
    }
}