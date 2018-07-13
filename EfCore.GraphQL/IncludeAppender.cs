using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace EfCoreGraphQL
{
    public static class IncludeAppender
    {
        public static IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, ResolveFieldContext<TItem> context)
            where TItem : class
        {
            return AddIncludes(query, context.SubFields);
        }
        public static IQueryable<TItem> AddIncludes<TItem,TSource>(IQueryable<TItem> query, ResolveFieldContext<TSource> context)
            where TItem : class
        {
            return AddIncludes(query, context.SubFields);
        }

        static IQueryable<T> AddIncludes<T>(IQueryable<T> query, IDictionary<string, Field> subFields) where T : class
        {
            foreach (var path in GetPaths(subFields.Values))
            {
                query = query.Include(path);
            }

            return query;
        }

        public static IEnumerable<string> GetPaths(IEnumerable<Field> fields)
        {
            var list = new List<string>();
            foreach (var field in fields)
            {
                AddField(list, field, null);
            }

            return list;
        }

        static void AddField(List<string> list, Field field, string parentPath)
        {
            string path;
            var fieldName = field.Name;
            fieldName = char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
            if (parentPath == null)
            {
                path = fieldName;
            }
            else
            {
                path = $"{parentPath}.{fieldName}";
            }

            var subFields = field.SelectionSet.Selections.OfType<Field>().ToList();
            if (!subFields.Any())
            {
                return;
            }

            list.Add(path);
            foreach (var subField in subFields)
            {
                AddField(list, subField, path);
            }
        }
    }
}