using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Type graphType,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Type graphType,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Type graphType,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField<object, TGraph, TReturn>(name, resolve);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField<TSource, TGraph, TReturn>(name, resolve);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField<TSource, TGraph, TReturn>(name, resolve);
            return graph.AddField(field);
        }

        FieldType BuildSingleField<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graphType), graphType);
            return BuildSingleField(name, resolve, graphType);
        }

        FieldType BuildSingleField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            return BuildSingleField(name, resolve, typeof(TGraph));
        }

        FieldType BuildSingleField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            return new FieldType
            {
                Name = name,
                Type = graphType,
                Arguments = new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" }),
                Resolver = new FuncFieldResolver<TSource, Task<TReturn>>(async context =>
                {
                    var returnTypes = resolve(context);
                    var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                    var id = context.GetArgument<string>("id");

                    var predicate = ExpressionBuilder<TReturn>.BuildPredicate("Id", Comparison.Equal, new []{ id });

                    var single = await withIncludes.FirstOrDefaultAsync(predicate, context.CancellationToken).ConfigureAwait(false);
                    return GlobalFilters.ShouldInclude(context.UserContext, single) ? single : null;
                })
            };
        }
    }
}
