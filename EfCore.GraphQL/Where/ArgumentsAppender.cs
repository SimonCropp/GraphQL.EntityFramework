using System.Collections.Generic;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentAppender
    {
        static QueryArgument<ListGraphType<WhereExpressionGraph>> wheresArgument = new QueryArgument<ListGraphType<WhereExpressionGraph>> { Name = "wheres" };
        static QueryArgument<WhereExpressionGraph> whereArgument = new QueryArgument<WhereExpressionGraph> { Name = "where" };

        public static readonly QueryArguments DefaultArguments = new QueryArguments(EnumerateArguments());

        public static void AddGraphQlArguments(this QueryArguments arguments)
        {
            foreach (var argument in EnumerateArguments())
            {
                arguments.Add(argument);
            }
        }
        public static IEnumerable<QueryArgument> EnumerateArguments()
        {
            yield return wheresArgument;
            yield return whereArgument;
        }
    }
}