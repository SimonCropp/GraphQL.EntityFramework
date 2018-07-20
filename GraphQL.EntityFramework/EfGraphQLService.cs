using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService
    {
        public EfGraphQLService(Dictionary<Type, List<string>> navigations)
        {
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