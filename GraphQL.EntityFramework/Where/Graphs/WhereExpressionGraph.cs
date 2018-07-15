using GraphQL.Types;

class WhereExpressionGraph : InputObjectGraphType<WhereExpression>
{
    public WhereExpressionGraph()
    {
        Field(x => x.Path);
        Field(x => x.Comparison);
        Field(x => x.Value, true);
    }
}