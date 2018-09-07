using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public interface IEfGraphQLService
    {
        FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class;

        FieldType AddQueryField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter =null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter =null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddNavigationField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;

        FieldType AddNavigationField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;

        FieldType AddNavigationField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddNavigationField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class;

        FieldType AddNavigationField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class;

        FieldType AddNavigationField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter=null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        ConnectionBuilder<TGraph, object> AddQueryConnectionField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        ConnectionBuilder<TGraph, object> AddNavigationConnectionField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        ConnectionBuilder<TGraph, TSource> AddNavigationConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;
    }
}