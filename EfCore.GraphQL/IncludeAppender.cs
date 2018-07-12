using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace EfCoreGraphQL
{
    public static class IncludeAppender
    {
        public static IEnumerable<string> GetPaths<T>(ResolveFieldContext<T> context)
        {
            return GetPaths(context.SubFields.Values);
        }

        public static IQueryable<T> AddIncludes<T>(IQueryable<T> query, ResolveFieldContext<T> context)
            where T : class
        {
            foreach (var path in GetPaths(context))
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
            if (parentPath == null)
            {
                path = field.Name;
            }
            else
            {
                path = $"{parentPath}.{field.Name}";
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
