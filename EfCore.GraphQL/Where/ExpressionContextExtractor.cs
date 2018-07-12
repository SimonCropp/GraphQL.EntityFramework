using System.Collections.Generic;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ExpressionContextExtractor
    {
        public static IEnumerable<WhereExpression> Read<T>(ResolveFieldContext<T> context)
        {
            foreach (var expression in context.GetArgument<WhereExpression[]>("wheres"))
            {
                yield return expression;
            }

            var whereExpression = context.GetArgument<WhereExpression>("where");
            if (whereExpression != null)
            {
                yield return whereExpression;
            }
        }
    }
}