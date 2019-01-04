using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService :
        IEfGraphQLService
    {
        public EfGraphQLService(DbContext dbContext)
        {
            Guard.AgainstNull(nameof(dbContext), dbContext);
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