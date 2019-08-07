using System;
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
        public FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddQueryFieldAsync(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }
        public FieldType AddQueryFieldAsync<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryFieldAsync(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddQueryFieldAsync(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddQueryFieldAsync<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryFieldAsync(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return AddQueryFieldAsync<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }
        public FieldType AddQueryFieldAsync<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryFieldAsync(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        FieldType BuildQueryFieldAsync<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            return BuildQueryFieldAsync(name, resolve, arguments, graphType);
        }

        FieldType BuildQueryFieldAsync<TSource, TReturn>(
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
            //create a list graph type based on the base field graph type
            var listGraphType = MakeListGraphType(graphType);
            //build the field
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                //append the default query arguments to the specified argument list
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                //custom resolve function
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
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
                        //query the database
                        var list = await withArguments.ToListAsync(context.CancellationToken);
                        //apply the global filter on each individually enumerated item
                        return await filters.ApplyFilter(list, context.UserContext);
                    })
            };
        }

        static List<string> emptyList = new List<string>();

        List<string> GetKeyNames<TSource>()
        {
            if (keyNames.TryGetValue(typeof(TSource), out var names))
            {
                return names;
            }

            return emptyList;
        }
    }
}