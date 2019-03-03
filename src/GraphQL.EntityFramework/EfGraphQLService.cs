using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService :
        IEfGraphQLService
    {
        GlobalFilters filters;

        public EfGraphQLService(DbContext dbContext, GlobalFilters filters)
        {
            Guard.AgainstNull(nameof(dbContext), dbContext);
            this.filters = filters;
            includeAppender = new IncludeAppender(NavigationReader.GetNavigationProperties(dbContext));
        }

        IncludeAppender includeAppender;

        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }
    }
}