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
        public static readonly QueryArgument<IntGraphType> SkipArgument =
            new QueryArgument<IntGraphType>
            {
                Name = "skip"
            };
        public static readonly QueryArgument<IntGraphType> TakeArgument =
            new QueryArgument<IntGraphType>
            {
                Name = "take"
            };

        public static readonly QueryArguments DefaultArguments;

        static ArgumentAppender()
        {
            DefaultArguments = new QueryArguments();
            AddGraphQlArguments(DefaultArguments);
        }

        public static void AddGraphQlArguments(this QueryArguments arguments)
        {
            arguments.Add(WhereArgument);
            arguments.Add(SkipArgument);
            arguments.Add(TakeArgument);
        }
    }
}