using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentAppender
    {
        public static readonly QueryArgument<ListGraphType<WhereExpressionGraph>> WhereArgument =
            new QueryArgument<ListGraphType<WhereExpressionGraph>>
            {
                Name = "where"
            };

        public static readonly QueryArguments DefaultArguments;

        static ArgumentAppender()
        {
            DefaultArguments = new QueryArguments
            {
                WhereArgument
            };
        }

        public static void AddGraphQlArguments(this QueryArguments arguments)
        {
            arguments.Add(WhereArgument);
        }
    }
}