using System.Collections.Generic;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentAppender
    {
        static QueryArgument<ListGraphType<WhereExpressionGraphType>> wheresArgument = new QueryArgument<ListGraphType<WhereExpressionGraphType>> { Name = "wheres" };
        static QueryArgument<WhereExpressionGraphType> whereArgument = new QueryArgument<WhereExpressionGraphType> { Name = "where" };

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