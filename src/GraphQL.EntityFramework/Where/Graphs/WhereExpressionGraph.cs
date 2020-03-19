using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class WhereExpressionGraph :
        InputObjectGraphType<WhereExpression>
    {
        public WhereExpressionGraph()
        {
            Field(x => x.Path, true);
            Field(x => x.Comparison, true);
            Field(x => x.Negate, true);
            Field(x => x.Case, true);
            Field(x => x.Value, true);
            Field(x => x.Connector, true);
            Field<ListGraphType<WhereExpressionGraph>>(
                name: "GroupedExpressions");
        }
    }
}