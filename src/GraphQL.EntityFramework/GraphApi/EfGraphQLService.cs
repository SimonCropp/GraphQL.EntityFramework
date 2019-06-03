using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService<TDbContext> :
        IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        GlobalFilters filters;
        DbContextFromUserContext<TDbContext> dbContextFromUserContext;
        Dictionary<Type, List<string>> keyNames = new Dictionary<Type, List<string>>();

        public EfGraphQLService(IModel model, GlobalFilters filters, DbContextFromUserContext<TDbContext> dbContextFromUserContext)
        {
            Guard.AgainstNull(nameof(model), model);
            Guard.AgainstNull(nameof(dbContextFromUserContext), dbContextFromUserContext);
            this.filters = filters;
            this.dbContextFromUserContext = dbContextFromUserContext;
            foreach (var entityType in model.GetEntityTypes())
            {
                var primaryKey = entityType.FindPrimaryKey();
                //This can happen for views
                if (primaryKey == null)
                {
                    continue;
                }

                var names = primaryKey.Properties.Select(x => x.Name).ToList();
                keyNames.Add(entityType.ClrType, names);
            }

            includeAppender = new IncludeAppender(NavigationReader.GetNavigationProperties(model));
        }

        IncludeAppender includeAppender;

        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }

        ResolveEfFieldContext<TDbContext, TSource> BuildEfContextFromGraphQlContext<TSource>(ResolveFieldContext<TSource> context)
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
                DbContext = dbContextFromUserContext(context.UserContext)
            };
        }
    }
}