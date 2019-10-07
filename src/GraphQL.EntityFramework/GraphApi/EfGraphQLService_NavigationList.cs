using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddNavigationListField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            //graphType should represent the graph type of the enumerated value, not the list graph type

            //build the navigation field
            var field = BuildNavigationField(graphType, name, resolve, includeNames, arguments);
            //add it to the graph
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            Type? graphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string>? includeNames,
            IEnumerable<QueryArgument>? arguments)
            where TReturn : class
        {
            //lookup the graph type if not explicitly specified
            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            //graphType represents the base field type, not the list graph type
            //create a list graph type based on the graph type specified
            var listGraphType = MakeListGraphType(graphType);
            //build the navigation field
            return BuildNavigationField(name, resolve, includeNames, listGraphType, arguments);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string>? includeNames,
            Type listGraphType,
            IEnumerable<QueryArgument>? arguments)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);

            //return the new field type
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                //take the arguments manually specified, if any, and append the query arguments
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                //add the metadata for the tables to be included in the query
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                //specify a custom resolve function
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    context =>
                    {
                        var efFieldContext = BuildContext(context);
                        //run the specified resolve function
                        var result = resolve(efFieldContext);
                        //apply any query filters specified in the arguments
                        result = result.ApplyGraphQlArguments(context);
                        //apply the global filter on each individually enumerated item
                        return efFieldContext.Filters.ApplyFilter(result, context.UserContext);
                    })
            };
        }
    }
}