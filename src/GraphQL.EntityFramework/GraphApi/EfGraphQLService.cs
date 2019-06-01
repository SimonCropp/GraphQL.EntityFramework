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
        Dictionary<Type, List<string>> keyNames = new Dictionary<Type, List<string>>();

        public EfGraphQLService(IModel model, GlobalFilters filters)
        {
            Guard.AgainstNull(nameof(model), model);
            this.filters = filters;
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
    }
}