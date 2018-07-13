using GraphQL.Types;

namespace EfCoreGraphQL
{
    public class WhereExpressionGraphType : InputObjectGraphType<WhereExpression>
    {
        public WhereExpressionGraphType()
        {
            Field(x => x.Path);
            Field(x => x.Comparison);
            Field(x => x.Value);
        }
    }
}