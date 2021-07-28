﻿using System;
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
        bool disableTracking;
        ResolveDbContext<TDbContext> resolveDbContext;
        IReadOnlyDictionary<Type, List<string>> keyNames;

        /// <param name="disableTracking">Use <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> for all <see cref="IQueryable{T}"/> operations.</param>
        public EfGraphQLService(
            IModel model,
            ResolveDbContext<TDbContext> resolveDbContext,
            ResolveFilters? resolveFilters = null,
            bool disableTracking = false)
        {
            this.resolveFilters = resolveFilters;
            this.disableTracking = disableTracking;

            this.resolveDbContext = resolveDbContext;

            keyNames = model.GetKeyNames();

            Navigations = NavigationReader.GetNavigationProperties(model);
            includeAppender = new(Navigations);
        }

        public IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> Navigations { get; }

        IncludeAppender includeAppender;

        ResolveEfFieldContext<TDbContext, TSource> BuildContext<TSource>(
            IResolveFieldContext<TSource> context)
        {
            return new()
            {
                UserContext = context.UserContext,
                Arguments = context.Arguments,
                CancellationToken = context.CancellationToken,
                Document = context.Document,
                Errors = context.Errors,
                FieldAst = context.FieldAst,
                FieldDefinition = context.FieldDefinition,
                Metrics = context.Metrics,
                Operation = context.Operation,
                ParentType = context.ParentType,
                Path = context.Path,
                RootValue = context.RootValue,
                Schema = context.Schema,
                Source = context.Source,
                SubFields = context.SubFields,
                Variables = context.Variables,
                DbContext = ResolveDbContext(context),
                Filters = ResolveFilter(context),
                Extensions = context.Extensions,
                RequestServices = context.RequestServices,
                ResponsePath = context.ResponsePath,
                ArrayPool = context.ArrayPool,
                Parent = context.Parent
            };
        }

        public TDbContext ResolveDbContext(IResolveFieldContext context)
        {
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