using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext ResolveDbContext(IResolveFieldContext context);

        IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
            where TItem : class;

        public IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> Navigations { get; }
    }
}