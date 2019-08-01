﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddSingleField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddSingleField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddSingleField<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        FieldType BuildSingleField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument> arguments,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);

            //lookup the graph type if not explicitly specified
            graphType = graphType ?? GraphTypeFinder.FindGraphType<TReturn>();
            //construct a non-null graph type for the specified graph type
            var wrappedType = typeof(NonNullGraphType<>).MakeGenericType(graphType);

            //build the field
            return new FieldType
            {
                Name = name,
                Type = wrappedType,
                //append the default query arguments to the specified argument list
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                //custom resolve function
                Resolver = new AsyncFieldResolver<TSource, TReturn>(
                    async context =>
                    {
                        //get field names of the table's primary key(s)
                        var names = GetKeyNames<TReturn>();
                        //run the specified resolve function
                        var returnTypes = await resolve(BuildEfContextFromGraphQlContext(context));
                        //include subtables in the query based on the metadata stored for the requested graph
                        var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                        //apply any query filters specified in the arguments
                        var withArguments = withIncludes.ApplyGraphQlArguments(context, names);
                        //run the query
                        var single = await withArguments.SingleOrDefaultAsync(context.CancellationToken);
                        //apply global filters to the returned value
                        if (single != null)
                        {
                            if (await filters.ShouldInclude(context.UserContext, single))
                            {
                                return single;
                            }
                        }
                        //throw an error if no value was found, or if the returned value was filtered out by the global filters
                        throw new ExecutionError("Not found");
                    })
            };
        }
    }
}