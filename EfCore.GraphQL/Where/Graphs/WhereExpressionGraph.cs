using GraphQL.Types;

namespace EfCoreGraphQL
{
    public class WhereExpressionGraph : InputObjectGraphType<WhereExpression>
    {
        public WhereExpressionGraph()
        {
            Field(x => x.Path);
            Field(x => x.Comparison);
            Field(x => x.Value, true);
            Field(x => x.Values,true);
        }
    }
}