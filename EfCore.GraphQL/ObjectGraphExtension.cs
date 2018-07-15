using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        static QueryArguments GetQueryArguments(QueryArguments arguments)
        {
            if (arguments == null)
            {
                return ArgumentAppender.DefaultArguments;
            }

            arguments.AddGraphQlArguments();

            return arguments;
        }
    }
}