using System;
using System.Collections.Generic;

namespace GraphQL.EntityFramework
{
    public class EfGraphQLService
    {
        public EfGraphQLService(Dictionary<Type, List<string>> navigations)
        {
            IncludeAppender = new IncludeAppender(navigations);
        }
        internal IncludeAppender IncludeAppender;
    }
}