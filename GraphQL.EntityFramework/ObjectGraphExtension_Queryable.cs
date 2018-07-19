using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static FieldType AddQueryField<TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField(efGraphQlService, graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddQueryField<TSource, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField(efGraphQlService,graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddQueryField<TSource, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildQueryField(efGraphQlService, graphType, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        static FieldType BuildQueryField<TSource, TReturn>(
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            string includeName)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(graphType);
            return BuildQueryField(efGraphQlService, name, resolve, arguments, includeName, listGraphType);
        }

        public static FieldType AddQueryField<TGraph, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<object, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddQueryField<TSource, TGraph, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddQueryField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildQueryField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName);
            return graph.AddField(field);
        }

        static FieldType BuildQueryField<TSource, TGraph, TReturn>(
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            string includeName)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(typeof(TGraph));
            return BuildQueryField(efGraphQlService,name, resolve, arguments, includeName, listGraphType);
        }

        static FieldType BuildQueryField<TSource, TReturn>(
            EfGraphQLService efGraphQlService,
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
                        return ExecuteAsyncQuery(name, listGraphType, context.Errors, () =>
                        {
                            var returnTypes = resolve(context);
                            return efGraphQlService.IncludeAppender.AddIncludes(returnTypes,context)
                                    .ApplyGraphQlArguments(context)
                                    .ToListAsync();
                        });
                    })
            };
        }
    }
}