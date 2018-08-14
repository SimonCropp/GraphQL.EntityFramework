using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService : IEfGraphQLService
    {
        public EfGraphQLService(Dictionary<Type, List<Navigation>> navigations)
        {
            Guard.AgainstNull(nameof(navigations), navigations);
            includeAppender = new IncludeAppender(navigations);
        }

        IncludeAppender includeAppender;

        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }
    }
}