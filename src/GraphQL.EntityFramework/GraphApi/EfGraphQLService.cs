using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService<TDbContext> :
        IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        ResolveFilters? resolveFilters;
        ResolveDbContext<TDbContext> resolveDbContext;
        IReadOnlyDictionary<Type, List<string>> keyNames;

        public EfGraphQLService(
            IModel model,
            ResolveDbContext<TDbContext> resolveDbContext,
            ResolveFilters? resolveFilters = null)
        {
            Guard.AgainstNull(nameof(model), model);
            Guard.AgainstNull(nameof(resolveDbContext), resolveDbContext);
            this.resolveFilters = resolveFilters;

            this.resolveDbContext = resolveDbContext;

            keyNames = model.GetKeyNames();

            Navigations = NavigationReader.GetNavigationProperties(model);
            includeAppender = new IncludeAppender(Navigations);
        }

        public IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> Navigations { get; }

        IncludeAppender includeAppender;

        ResolveEfFieldContext<TDbContext, TSource> BuildContext<TSource>(
            IResolveFieldContext<TSource> context)
        {
            return new ResolveEfFieldContext<TDbContext, TSource>
            {
                UserContext = context.UserContext,
                Arguments = context.Arguments,
                CancellationToken = context.CancellationToken,
                Document = context.Document,
                Errors = context.Errors,
                FieldAst = context.FieldAst,
                FieldDefinition = context.FieldDefinition,
                FieldName = context.FieldName,
                Fragments = context.Fragments,
                Metrics = context.Metrics,
                Operation = context.Operation,
                ParentType = context.ParentType,
                Path = context.Path,
                ReturnType = context.ReturnType,
                RootValue = context.RootValue,
                Schema = context.Schema,
                Source = context.Source,
                SubFields = context.SubFields,
                Variables = context.Variables,
                DbContext = ResolveDbContext(context),
                Filters = ResolveFilter(context)
            };
        }

        public TDbContext ResolveDbContext(IResolveFieldContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            return resolveDbContext(context.UserContext);
        }

        Filters ResolveFilter<TSource>(IResolveFieldContext<TSource> context)
        {
            var filter = resolveFilters?.Invoke(context.UserContext);
            return filter ?? NullFilters.Instance;
        }

        public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
            where TItem : class
        {
            return includeAppender.AddIncludes(query, context);
        }
    }
}