using GraphQL.Builders;
using GraphQL.Types;

static class ArgumentAppender
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

    public static void AddWhereArgument<TSourceType, TGraphType>(this ConnectionBuilder<TGraphType, TSourceType> builder)
        where TGraphType : IGraphType
    {
        builder.Argument<ListGraphType<WhereExpressionGraph>>("where", null);
    }

    public static QueryArguments GetQueryArguments()
    {
        var arguments = new QueryArguments();
        arguments.AddGraphQlArguments();
        return arguments;
    }

    public static void AddGraphQlArguments(this QueryArguments arguments)
    {
        arguments.Add(WhereArgument);
        arguments.Add(SkipArgument);
        arguments.Add(TakeArgument);
    }
}