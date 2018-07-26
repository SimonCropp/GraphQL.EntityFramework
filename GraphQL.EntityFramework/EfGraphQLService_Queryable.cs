using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService
    {
        public FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField( graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField(graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField( graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            string includeName)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(graphType);
            return BuildQueryField( name, resolve, arguments, includeName, listGraphType);
        }

        public FieldType AddQueryField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<object, TGraph, TReturn>( name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<TSource, TGraph, TReturn>( name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<TSource, TGraph, TReturn>( name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        FieldType BuildQueryField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            string includeName)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(typeof(TGraph));
            return BuildQueryField(name, resolve, arguments, includeName, listGraphType);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            string includeName,
            Type listGraphType)
            where TReturn : class
        {
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(includeName),
                Resolver = new AsyncFieldResolver<TSource, List<TReturn>>(
                    context =>
                    {
                        var returnTypes = resolve(context);
                        return includeAppender.AddIncludes(returnTypes, context)
                            .ApplyGraphQlArguments(context)
                            .ToListAsync();
                    })
            };
        }
    }
}